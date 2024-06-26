using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using AppLib.ServiceBus.Services.IService;
using BimshireStore.Services.OrderAPI.Data;
using BimshireStore.Services.OrderAPI.Models;
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Models.Extensions;
using BimshireStore.Services.OrderAPI.Services.IService;
using BimshireStore.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.OrderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]  // hard coded route name
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IProductService _productService;
        private readonly IConfiguration _config;
        private readonly IServiceBusProducer _sbp;

        public OrderController(ApplicationDbContext db, IProductService productService, IConfiguration config, IServiceBusProducer sbp)
        {
            _db = db;
            _productService = productService;
            _config = config;
            _sbp = sbp;
        }

        [Authorize]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            try
            {
                var userId = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                IEnumerable<OrderHeader> orderHeaders;

                if (User.IsInRole(SD.Role_Admin))
                {
                    orderHeaders = await _db.OrderHeaders.Include(x => x.OrderDetails).OrderByDescending(x => x.OrderHeaderId).ToListAsync();
                }
                else
                {
                    orderHeaders = await _db.OrderHeaders.Include(x => x.OrderDetails).Where(x => x.UserId == userId).OrderByDescending(x => x.OrderHeaderId).ToListAsync();
                }

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = orderHeaders.Select(x => x.ToDto()),
                        StatusCode = System.Net.HttpStatusCode.OK
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize]
        [HttpGet("{orderId:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetById([FromRoute] int orderId)
        {
            try
            {
                var userId = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                OrderHeader? orderHeader;

                if (User.IsInRole(SD.Role_Admin))
                {
                    orderHeader = await _db.OrderHeaders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderHeaderId == orderId);
                }
                else
                {
                    orderHeader = await _db.OrderHeaders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.UserId == userId && x.OrderHeaderId == orderId);
                }

                if (orderHeader is not null)
                {
                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            Result = orderHeader.ToDto(),
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }

                return NotFound(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = ["Order not found"],
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateOrderAsync([FromBody] CartDto cart)
        {
            try
            {
                var orderHeaderDto = cart.CartHeader.ToDto();
                orderHeaderDto.OrderTime = DateTime.UtcNow;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = cart.CartDetail?.Select(x => x.ToDto()) ?? [];

                var orderHeader = JsonSerializer.Deserialize<OrderHeader>(JsonSerializer.Serialize(orderHeaderDto), SD.JsonSerializerConfig.DefaultOptions);

                if (orderHeader is not null)
                {
                    await _db.OrderHeaders.AddAsync(orderHeader);
                    await _db.SaveChangesAsync();

                    orderHeaderDto.OrderHeaderId = orderHeader.OrderHeaderId;

                    return Ok(
                            new ApiResponse
                            {
                                IsSuccess = true,
                                Result = orderHeaderDto,
                                StatusCode = System.Net.HttpStatusCode.OK
                            });
                }

                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = ["Failed to create order"],
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize]
        [HttpPost("create-stripe-session")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateStripeSessionAsync([FromBody] StripeRequest stripeRequest)
        {
            try
            {
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequest.ApprovedUrl,
                    CancelUrl = stripeRequest.CancelledUrl,
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var lineItem in stripeRequest.OrderHeaderDto.OrderDetails)
                {
                    var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(lineItem.Price * 100), // e.g. $20.99 - > 2099
                            Currency = "usd",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = lineItem.ProductName
                            },
                        },
                        Quantity = lineItem.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                if (stripeRequest.OrderHeaderDto.Discount > 0)
                {
                    // Apply any discounts
                    var Discounts = new List<Stripe.Checkout.SessionDiscountOptions>()
                    {
                        new Stripe.Checkout.SessionDiscountOptions
                        {
                            Coupon = stripeRequest.OrderHeaderDto.CouponCode
                        }
                    };

                    options.Discounts = Discounts;
                }

                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session stripeSession = service.Create(options);
                stripeRequest.StripeSessionUrl = stripeSession.Url;
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == stripeRequest.OrderHeaderDto.OrderHeaderId);
                orderHeader.StripeSessionId = stripeSession.Id;
                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = stripeRequest,
                        StatusCode = System.Net.HttpStatusCode.OK
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize]
        [HttpPost("validate-stripe-session")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> ValidateStripeSessionAsync([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderHeaderId);

                if (orderHeader is not null)
                {
                    var service = new Stripe.Checkout.SessionService();
                    Stripe.Checkout.Session stripeSession = await service.GetAsync(orderHeader.StripeSessionId);

                    var paymentIntentService = new Stripe.PaymentIntentService();
                    var paymentIntent = await paymentIntentService.GetAsync(stripeSession.PaymentIntentId);
                    if (paymentIntent is not null && paymentIntent.Status == "succeeded")
                    {
                        // payment was successful
                        orderHeader.PaymentIntentId = paymentIntent.Id;
                        orderHeader.Status = SD.Status_Approved;
                        await _db.SaveChangesAsync();

                        var reward = new RewardDto
                        {
                            UserId = orderHeader.UserId,
                            OrderId = orderHeader.OrderHeaderId,
                            RewardActivity = (int)Math.Floor(orderHeader.OrderTotal), // 1 reward point per dollar
                            RewardDate = DateTime.UtcNow
                        };

                        _sbp.SendMessageToExchange(
                            reward,
                            _config.GetValue<string>("MessageBus:TopicAndQueueNames:OrderApprovedExchange")
                                ?? throw new InvalidOperationException("Invalid MessageBus Topic/Queue Name")
                        );

                        return Ok(
                            new ApiResponse
                            {
                                IsSuccess = true,
                                Result = orderHeader.ToDto(),
                                StatusCode = System.Net.HttpStatusCode.OK
                            }
                        );
                    }
                    else
                    {
                        return BadRequest(
                            new ApiResponse
                            {
                                IsSuccess = false,
                                ErrorMessages = [$"Payment failed: {paymentIntent?.Status}"],
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            }
                        );
                    }
                }
                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = ["Payment failed"],
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    }
                );
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize]
        [HttpPost("{orderId:int}/set-status/{newStatus}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> SetStatus([FromRoute] int orderId, [FromRoute] string newStatus)
        {
            try
            {
                var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == orderId);

                if (orderHeader is not null)
                {
                    if (newStatus == SD.Status_Cancelled)
                    {
                        // refund customer
                        var options = new Stripe.RefundCreateOptions
                        {
                            Reason = Stripe.RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId
                        };

                        var service = new Stripe.RefundService();
                        var refund = service.CreateAsync(options);
                    }

                    orderHeader.Status = newStatus;
                    await _db.SaveChangesAsync();

                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }

                return NotFound(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = ["Order not found"],
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }
}
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
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] CartDto cart)
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
        public async Task<ActionResult<ApiResponse>> CreateStripeSession([FromBody] StripeRequest stripeRequest)
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
        public async Task<ActionResult<ApiResponse>> ValidateStripeSession([FromBody] int orderHeaderId)
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

    }
}
using System.Text.Json;
using BimshireStore.Services.OrderAPI.Data;
using BimshireStore.Services.OrderAPI.Models;
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Models.Extensions;
using BimshireStore.Services.OrderAPI.Services.IService;
using BimshireStore.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using static BimshireStore.Services.OrderAPI.Utility.SD;

namespace BimshireStore.Services.OrderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]  // hard coded route name
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IProductService _productService;

        public OrderController(ApplicationDbContext db, IProductService productService)
        {
            _db = db;
            _productService = productService;
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

                var orderHeader = JsonSerializer.Deserialize<OrderHeader>(JsonSerializer.Serialize(orderHeaderDto), JsonSerializerConfig.DefaultOptions);

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
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(lineItem.Price * 100), // e.g. $20.99 - > 2099
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = lineItem.ProductName
                            },
                        },
                        Quantity = lineItem.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                Session stripeSession = service.Create(options);
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

    }
}
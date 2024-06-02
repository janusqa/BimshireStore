using System.Text.Json;
using BimshireStore.Services.OrderAPI.Data;
using BimshireStore.Services.OrderAPI.Models;
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Models.Extensions;
using BimshireStore.Services.OrderAPI.Services.IService;
using BimshireStore.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    }
}
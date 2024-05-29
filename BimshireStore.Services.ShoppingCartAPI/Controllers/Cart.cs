using BimshireStore.Services.ShoppingCartAPI.Data;
using BimshireStore.Services.ShoppingCartAPI.Models;
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.ShoppingCartAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/carts")]  // hard coded route name
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Upsert([FromBody] CartDto cart)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == cart.CartHeader.UserId);
                if (cartHeaderFromDb is null)
                {
                    // Create CardHeader 
                    var newCartHeader = new CartHeader
                    {
                        UserId = cart.CartHeader.UserId,
                        CouponCode = cart.CartHeader.CouponCode,
                    };

                    await _db.CartHeaders.AddAsync(newCartHeader);
                    await _db.SaveChangesAsync();

                    // Create CartDetail
                    if (cart.CartDetail is not null)
                    {
                        cart.CartDetail.First().CartHeaderId = newCartHeader.CartHeaderId;

                        var tempCartDetail = cart.CartDetail.First();
                        var newCartDetail = new CartDetail
                        {
                            CartHeaderId = tempCartDetail.CartHeaderId,
                            ProductId = tempCartDetail.ProductId,
                            Count = tempCartDetail.Count,
                        };

                        await _db.CartDetails.AddAsync(newCartDetail);
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    // Check if product already has a CartDetail
                    var productId = cart.CartDetail?.FirstOrDefault()?.ProductId;
                    var cartDetailFromDb = await _db.CartDetails.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ProductId == (productId ?? 0) && x.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    if (cartDetailFromDb is null)
                    {
                        // Create CartDetail
                        if (cart.CartDetail is not null)
                        {
                            cart.CartDetail.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

                            var tempCartDetail = cart.CartDetail.First();
                            var newCartDetail = new CartDetail
                            {
                                CartHeaderId = tempCartDetail.CartHeaderId,
                                ProductId = tempCartDetail.ProductId,
                                Count = tempCartDetail.Count,
                            };

                            await _db.CartDetails.AddAsync(newCartDetail);
                            await _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Update Cart Detail
                        if (cart.CartDetail is not null)
                        {
                            cartDetailFromDb.Count = cart.CartDetail.First().Count;
                            _db.CartDetails.Update(cartDetailFromDb);
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = cart,
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
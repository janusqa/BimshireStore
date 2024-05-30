using System.Text.Json;
using BimshireStore.Services.ShoppingCartAPI.Data;
using BimshireStore.Services.ShoppingCartAPI.Models;
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;
using BimshireStore.Services.ShoppingCartAPI.Models.Extensions;
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BimshireStore.Services.ShoppingCartAPI.Utility.SD;

namespace BimshireStore.Services.ShoppingCartAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/carts")]  // hard coded route name
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IProductService _ps;
        private readonly ICouponService _cs;


        public CartController(ApplicationDbContext db, IProductService ps, ICouponService cs)
        {
            _db = db;
            _ps = ps;
            _cs = cs;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetCart([FromRoute] string userId)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.Where(x => x.UserId == userId).FirstOrDefaultAsync();
                if (cartHeaderFromDb is not null)
                {
                    var cartDetailsFromDb = _db.CartDetails.Where(x => x.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    var products = new List<ProductDto>();
                    var productApiResponse = await _ps.GetAllAsync();
                    if (productApiResponse is not null && productApiResponse.IsSuccess)
                    {
                        products = JsonSerializer.Deserialize<List<ProductDto>>(JsonSerializer.Serialize(productApiResponse.Result), JsonSerializerConfig.DefaultOptions);
                    }

                    // Calculate Cart total
                    foreach (var item in cartDetailsFromDb)
                    {
                        item.Product = products?.FirstOrDefault(x => x.ProductId == item.ProductId);
                        cartHeaderFromDb.CartTotal += item.Count * (item.Product?.Price ?? 0);
                    }

                    // Apply a coupon if available
                    if (!string.IsNullOrWhiteSpace(cartHeaderFromDb.CouponCode))
                    {
                        var couponApiresponse = await _cs.GetByCodeAsync(cartHeaderFromDb.CouponCode);
                        if (couponApiresponse is not null && couponApiresponse.IsSuccess)
                        {
                            var coupon = JsonSerializer.Deserialize<CouponDto>(JsonSerializer.Serialize(couponApiresponse.Result), JsonSerializerConfig.DefaultOptions);
                            if (coupon is not null && cartHeaderFromDb.CartTotal >= coupon.MinAmount)
                            {
                                cartHeaderFromDb.Discount = coupon.DiscountAmount;
                                cartHeaderFromDb.CartTotal -= coupon.DiscountAmount;
                            }
                        }
                    }

                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            Result = new CartDto { CartHeader = cartHeaderFromDb.ToDto(), CartDetail = cartDetailsFromDb.Select(x => x.ToDto()) },
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }
                return NotFound(
                    new ApiResponse
                    {
                        IsSuccess = false,
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

        [HttpPost("upsert-item")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpsertItem([FromBody] CartDto cart)
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

        [HttpPost("remove-item")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> RemoveItem([FromBody] int cartDetailId)
        {
            try
            {
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(x => x.CartDetailId == cartDetailId);
                if (cartDetailsFromDb is not null)
                {
                    var itemCount = _db.CartDetails.AsNoTracking().Where(x => x.CartHeaderId == cartDetailsFromDb.CartHeaderId).Count();
                    _db.CartDetails.Remove(cartDetailsFromDb);
                    if (itemCount == 1)
                    {
                        var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(x => x.CartHeaderId == cartDetailsFromDb.CartHeaderId);
                        if (cartHeaderToRemove is not null) _db.CartHeaders.Remove(cartHeaderToRemove);
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
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

        [HttpPost("apply-coupon")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> ApplyCoupon([FromBody] CartDto cart)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == cart.CartHeader.UserId);

                if (cartHeaderFromDb is not null)
                {
                    cartHeaderFromDb.CouponCode = cart.CartHeader.CouponCode;
                    _db.CartHeaders.Update(cartHeaderFromDb);
                    await _db.SaveChangesAsync();

                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            StatusCode = System.Net.HttpStatusCode.NotFound
                        });
                }
                return NotFound(
                      new ApiResponse
                      {
                          IsSuccess = false,
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
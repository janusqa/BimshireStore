using BimshireStore.Services.CouponAPI.Data;
using BimshireStore.Services.CouponAPI.Models;
using BimshireStore.Services.CouponAPI.Models.Dto;
using BimshireStore.Services.CouponAPI.Models.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.CouponAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/coupons")]  // hard coded route name
    public class CouponController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get()
        {
            try
            {
                var coupons = (await _db.Coupons.ToListAsync()).Select(x => x.ToDto());

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = coupons,
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

        [HttpGet("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get([FromRoute] int Id)
        {
            try
            {
                var coupon = (await _db.Coupons.FirstOrDefaultAsync(x => x.CouponId == Id))?.ToDto();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = coupon,
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

        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get([FromRoute] string code)
        {
            try
            {
                var coupon = (await _db.Coupons.FirstOrDefaultAsync(x => !string.IsNullOrWhiteSpace(x.CouponCode) && x.CouponCode.ToLower() == code.ToLower()))?.ToDto();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = coupon,
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

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Post([FromBody] CouponDto couponDto)
        {
            try
            {
                var coupon = new Coupon
                {
                    DiscountAmount = couponDto.DiscountAmount,
                    MinAmount = couponDto.MinAmount,
                    CouponCode = couponDto.CouponCode
                };
                await _db.Coupons.AddAsync(coupon);
                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = coupon.ToDto(),
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

        [HttpPut("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Put([FromBody] CouponDto couponDto)
        {
            try
            {
                var coupon = new Coupon
                {
                    CouponId = couponDto.CouponId,
                    DiscountAmount = couponDto.DiscountAmount,
                    MinAmount = couponDto.MinAmount,
                    CouponCode = couponDto.CouponCode
                };
                _db.Coupons.Update(coupon);
                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = coupon.ToDto(),
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

        [HttpDelete("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Delete([FromRoute] int Id)
        {
            try
            {
                var coupon = await _db.Coupons.FirstOrDefaultAsync(x => x.CouponId == Id);
                if (coupon is not null)
                {
                    _db.Coupons.Remove(coupon);
                    await _db.SaveChangesAsync();


                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            Result = coupon.ToDto(),
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }
                else
                {
                    return NotFound(new ApiResponse { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.NotFound });
                }
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
using BimshireStore.Services.CouponAPI.Models;
using BimshireStore.Services.CouponAPI.Models.Dto;

namespace AppLib.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static CouponDto ToDto(this Coupon Coupon)
        {
            return new CouponDto
            {
                CouponId = Coupon.CouponId,
                CouponCode = Coupon.CouponCode ?? string.Empty,
                DiscountAmount = Coupon.DiscountAmount,
                MinAmount = Coupon.MinAmount
            };
        }

    }
}
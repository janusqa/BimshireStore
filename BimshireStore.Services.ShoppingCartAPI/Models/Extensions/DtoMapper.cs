
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static CartHeaderDto ToDto(this CartHeader cartHeader)
        {
            return new CartHeaderDto
            {
                CartHeaderId = cartHeader.CartHeaderId,
                UserId = cartHeader.UserId,
                CouponCode = cartHeader.CouponCode,
                Discount = cartHeader.Discount,
                CartTotal = cartHeader.CartTotal,
            };
        }

        public static CartDetailDto ToDto(this CartDetail cartDetail)
        {
            return new CartDetailDto
            {
                CartDetailId = cartDetail.CartDetailId,
                CartHeaderId = cartDetail.CartHeaderId,
                CartHeader = cartDetail.CartHeader?.ToDto(),
                ProductId = cartDetail.ProductId,
                Product = cartDetail.Product,
                Count = cartDetail.Count,
            };
        }

    }
}
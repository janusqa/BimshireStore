
using BimshireStore.Services.OrderAPI.Models.Dto;

namespace BimshireStore.Services.OrderAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static OrderHeaderDto ToDto(this OrderHeader orderHeader)
        {
            return new OrderHeaderDto
            {
                OrderHeaderId = orderHeader.OrderHeaderId,
                UserId = orderHeader.UserId,
                Name = orderHeader.Name,
                Phone = orderHeader.Phone,
                Email = orderHeader.Email,
                CouponCode = orderHeader.CouponCode,
                Discount = orderHeader.Discount,
                OrderTotal = orderHeader.OrderTotal,
                OrderTime = orderHeader.OrderTime,
                Status = orderHeader.Status,
                PaymentIntentId = orderHeader.PaymentIntentId,
                StripeSessionId = orderHeader.StripeSessionId,
                OrderDetails = orderHeader.OrderDetails?.Select(x => x.ToDto()) ?? []
            };
        }

        public static OrderHeaderDto ToDto(this CartHeaderDto cartHeaderDto)
        {
            return new OrderHeaderDto
            {
                UserId = cartHeaderDto.UserId,
                Name = cartHeaderDto.Name,
                Phone = cartHeaderDto.Phone,
                Email = cartHeaderDto.Email,
                CouponCode = cartHeaderDto.CouponCode,
                Discount = cartHeaderDto.Discount,
                OrderTotal = cartHeaderDto.CartTotal,
            };
        }

        public static OrderDetailDto ToDto(this OrderDetail orderDetail)
        {
            return new OrderDetailDto
            {
                OrderDetailId = orderDetail.OrderDetailId,
                OrderHeaderId = orderDetail.OrderHeaderId,
                ProductId = orderDetail.ProductId,
                Product = orderDetail.Product,
                ProductName = orderDetail.Product?.Name ?? string.Empty,
                Price = orderDetail.Price,
                Count = orderDetail.Count,
            };
        }

        public static OrderDetailDto ToDto(this CartDetailDto cartDetailDto)
        {
            return new OrderDetailDto
            {
                ProductId = cartDetailDto.ProductId,
                Product = cartDetailDto.Product,
                ProductName = cartDetailDto.Product?.Name ?? string.Empty,
                Price = cartDetailDto.Product?.Price ?? 0,
                Count = cartDetailDto.Count,
            };
        }
    }
}
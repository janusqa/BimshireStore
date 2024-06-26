using BimshireStore.Services.ProductAPI.Models.Dto;

namespace BimshireStore.Services.ProductAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static ProductDto ToDto(this Product Product)
        {
            return new ProductDto
            {
                ProductId = Product.ProductId,
                Name = Product.Name,
                Price = Product.Price,
                Description = Product.Description,
                CategoryName = Product.CategoryName,
                ImageUrl = Product.ImageUrl,
                ImageLocalPath = Product.ImageLocalPath
            };
        }

    }
}
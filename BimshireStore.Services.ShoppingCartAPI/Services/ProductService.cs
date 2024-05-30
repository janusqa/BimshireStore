using BimshireStore.Services.ShoppingCartAPI.Models.Dto;
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Utility;

namespace BimshireStore.Services.ShoppingCartAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _bs;

        public ProductService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> GetAllAsync()
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.ProductApiBaseAddress}api/products"
            });
        }
    }
}
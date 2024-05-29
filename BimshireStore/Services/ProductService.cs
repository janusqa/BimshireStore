using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
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
                Url = $"{SD.CouponApiBaseAddress}api/products"
            });
        }

        public async Task<ApiResponse?> GetByIdAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/products/{id}"
            });
        }

        public async Task<ApiResponse?> CreateAsync(ProductDto product)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.CouponApiBaseAddress}api/products",
                Data = product
            });
        }

        public async Task<ApiResponse?> UpdateAsync(ProductDto product)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.PUT,
                Url = $"{SD.CouponApiBaseAddress}api/products",
                Data = product
            });
        }

        public async Task<ApiResponse?> DeleteAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.DELETE,
                Url = $"{SD.CouponApiBaseAddress}api/products/{id}"
            });
        }
    }
}
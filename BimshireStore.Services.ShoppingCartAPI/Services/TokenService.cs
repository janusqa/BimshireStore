
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Utility;

namespace BimshireStore.Services.ShoppingCartAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _ihca;

        public TokenService(IHttpContextAccessor ihca)
        {
            _ihca = ihca;
        }

        public void ClearToken()
        {
            _ihca.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;
            var hasToken = _ihca.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);

            return hasToken is true ? token : null;
        }

        public void SetToken(string token)
        {
            _ihca.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
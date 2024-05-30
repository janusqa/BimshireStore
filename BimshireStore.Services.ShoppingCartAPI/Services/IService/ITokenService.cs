namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{
    public interface ITokenService
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
    }
}
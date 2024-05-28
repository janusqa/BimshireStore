namespace BimshireStore.Services.IService
{
    public interface ITokenService
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
    }
}
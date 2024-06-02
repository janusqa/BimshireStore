namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{

    // This class implements api to api authn/authz via the DelegatingHandler
    // Delegating Handlers are similar to .NET Core Middleware but operated
    // on the Client Side. So If we are making an http request using 
    // HTTPClient, we can leverage the DelegatingHandler to pass our Bearer
    // Token to the other request.
    public interface IServiceAccount
    {

    }
}
using System.Net.Http.Headers;
using BimshireStore.Services.OrderCartAPI.Services.IService;
using Microsoft.AspNetCore.Authentication;

namespace BimshireStore.Services.OrderAPI.Services
{

    // This class implements api to api authn/authz via the DelegatingHandler
    // Delegating Handlers are similar to .NET Core Middleware but operated
    // on the Client Side. So If we are making an http request using 
    // HTTPClient, we can leverage the DelegatingHandler to pass our Bearer
    // Token to the other request.
    public class ServiceAccount : DelegatingHandler, IServiceAccount
    {
        private readonly IHttpContextAccessor _accessor;
        public ServiceAccount(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _accessor.HttpContext;

            if (context is not null)
            {
                var token = await context.GetTokenAsync("access_token"); // access_token is a magic string
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }

    }
}
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace AspireApp.Web
{
    public class AuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext ??
                throw new InvalidOperationException("No HttpContext available from the IHttpContextAccessor!");

            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // If unauthorized, redirect to login
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (httpContext.Response != null)
                {
                    httpContext.Response.Redirect("/authentication/login");
                }
            }

            return response;
        }
    }
}

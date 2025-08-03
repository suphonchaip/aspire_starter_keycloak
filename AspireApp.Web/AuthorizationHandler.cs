using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace AspireApp.Web
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            string? accessToken = null;

            // 1. ลองหา server-side token ก่อน (Keycloak cookies)
            try
            {
                accessToken = await httpContext.GetTokenAsync("access_token");
            }
            catch
            {
                // Ignore errors
            }

            // 2. หากไม่มี server-side token ลองหาใน HttpContext.Items (ที่เก็บจาก session storage)
            if (string.IsNullOrEmpty(accessToken))
            {
                if (httpContext.Items.TryGetValue("client_access_token", out var clientToken))
                {
                    accessToken = clientToken?.ToString();
                }
            }

            // 3. เพิ่ม Authorization header หากมี token
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

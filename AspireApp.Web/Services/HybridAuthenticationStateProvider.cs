using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AspireApp.Web.Services
{
    public interface ITokenService
    {
        Task<string?> GetAccessTokenAsync();
        Task SetAccessTokenAsync(string token);
        Task ClearTokensAsync();
    }

    public class TokenService : ITokenService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IJSRuntime jsRuntime, IHttpContextAccessor httpContextAccessor)
        {
            _jsRuntime = jsRuntime;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            try
            {
                // ลองหา server-side token ก่อน
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    try
                    {
                        var serverToken = await httpContext.GetTokenAsync("access_token");
                        if (!string.IsNullOrEmpty(serverToken))
                        {
                            return serverToken;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                // ลองหา client-side token
                var clientToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "access_token");
                return string.IsNullOrEmpty(clientToken) ? null : clientToken;
            }
            catch
            {
                return null;
            }
        }

        public async Task SetAccessTokenAsync(string token)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "access_token", token);
                
                // เก็บไว้ใน HttpContext.Items สำหรับ AuthorizationHandler
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["client_access_token"] = token;
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        public async Task ClearTokensAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "access_token");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refresh_token");
                
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items.Remove("client_access_token");
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }

    public class HybridAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IServiceProvider _serviceProvider;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public HybridAuthenticationStateProvider(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
        {
            _jsRuntime = jsRuntime;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // ลำดับความสำคัญ:
                // 1. ตรวจสอบ Server-side authentication ก่อน (Keycloak cookies)
                // 2. ตรวจสอบ JWT token ใน session storage (Form login)

                // 1. ตรวจสอบ Server-side authentication
                try
                {
                    var httpContext = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
                    if (httpContext?.User?.Identity?.IsAuthenticated == true)
                    {
                        _currentUser = httpContext.User;
                        return new AuthenticationState(_currentUser);
                    }
                }
                catch
                {
                    // Ignore server-side errors
                }

                // 2. ตรวจสอบ JWT token ใน session storage
                var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "access_token");
                
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var user = CreateUserFromToken(token);
                    if (user.Identity?.IsAuthenticated == true)
                    {
                        _currentUser = user;
                        
                        // เก็บ token ใน HttpContext.Items สำหรับ AuthorizationHandler
                        var httpContext = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
                        if (httpContext != null)
                        {
                            httpContext.Items["client_access_token"] = token;
                        }
                        
                        return new AuthenticationState(_currentUser);
                    }
                }

                // 3. ถ้าไม่มีทั้งคู่ return anonymous
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(_currentUser);
            }
            catch (InvalidOperationException)
            {
                // JSRuntime ยังไม่พร้อม (เช่น ใน prerendering)
                return new AuthenticationState(_currentUser);
            }
            catch (Exception)
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(_currentUser);
            }
        }

        private ClaimsPrincipal CreateUserFromToken(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                
                if (!jwtHandler.CanReadToken(token))
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }

                var jwtToken = jwtHandler.ReadJwtToken(token);
                
                // ตรวจสอบว่า token หมดอายุหรือยัง
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }

                var claims = jwtToken.Claims.ToList();
                
                // เพิ่ม name claim หากไม่มี
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    var preferredUsername = claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                    var sub = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                    var nameValue = preferredUsername ?? sub ?? "Unknown";
                    claims.Add(new Claim(ClaimTypes.Name, nameValue));
                }
                
                var identity = new ClaimsIdentity(claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            try
            {
                var user = CreateUserFromToken(token);
                if (user.Identity?.IsAuthenticated == true)
                {
                    _currentUser = user;
                    
                    // เก็บ token ใน HttpContext.Items
                    var httpContext = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
                    if (httpContext != null)
                    {
                        httpContext.Items["client_access_token"] = token;
                    }
                    
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
                }
                else
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
                }
            }
            catch (Exception)
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            }
        }

        public void NotifyUserLogout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            
            // ล้าง token จาก HttpContext.Items
            var httpContext = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items.Remove("client_access_token");
            }
            
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
        
        public async Task<bool> HasSessionTokenAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "access_token");
                return !string.IsNullOrWhiteSpace(token);
            }
            catch
            {
                return false;
            }
        }

        // เพิ่ม method สำหรับ force refresh authentication state
        public async Task RefreshAuthenticationStateAsync()
        {
            try
            {
                var authState = await GetAuthenticationStateAsync();
                _currentUser = authState.User;
                NotifyAuthenticationStateChanged(Task.FromResult(authState));
            }
            catch (Exception)
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            }
        }
    }
}
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AspireApp.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "access_token");
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                    return new AuthenticationState(_currentUser);
                }

                var jwtHandler = new JwtSecurityTokenHandler();
                
                if (jwtHandler.CanReadToken(token))
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);
                    
                    // ตรวจสอบว่า token หมดอายุหรือยัง
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "access_token");
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refresh_token");
                        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                        return new AuthenticationState(_currentUser);
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
                    _currentUser = new ClaimsPrincipal(identity);
                    return new AuthenticationState(_currentUser);
                }

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

        public void NotifyUserAuthentication(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                
                if (jwtHandler.CanReadToken(token))
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);
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
                    _currentUser = new ClaimsPrincipal(identity);
                    
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
                }
            }
            catch (Exception)
            {
                // หากมีข้อผิดพลาด ให้ fallback เป็น anonymous
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            }
        }

        public void NotifyUserLogout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}
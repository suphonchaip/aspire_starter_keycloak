using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AspireApp.Web
{
    internal static class LoginLogoutEndpointRouteBuilderExtensions
    {
        internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("authentication");

            group.MapGet("/login", (string? returnUrl) => 
                {
                    var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
                    return TypedResults.Challenge(new AuthenticationProperties { RedirectUri = redirectUri });
                })
                .AllowAnonymous();

            // รองรับทั้ง GET และ POST สำหรับ logout
            group.MapGet("/logout", (string? returnUrl) => 
                {
                    var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
                    return TypedResults.SignOut(new AuthenticationProperties { RedirectUri = redirectUri },
                        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
                })
                .AllowAnonymous();

            group.MapPost("/logout", (string? returnUrl) => 
                {
                    var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
                    return TypedResults.SignOut(new AuthenticationProperties { RedirectUri = redirectUri },
                        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
                })
                .AllowAnonymous();

            // เพิ่ม signout callback endpoint
            group.MapGet("/signout-callback-oidc", (string? returnUrl) =>
                {
                    var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
                    return TypedResults.Redirect(redirectUri);
                })
                .AllowAnonymous();

            return group;
        }
    }
}

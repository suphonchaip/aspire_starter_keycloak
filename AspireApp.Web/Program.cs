using AspireApp.Web;
using AspireApp.Web.Components;
using AspireApp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
builder.AddSeqEndpoint("seq");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();

// Add Keycloak Auth Service
builder.Services.AddHttpClient<IKeycloakAuthService, KeycloakAuthService>();

// Add Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    })
    .AddHttpMessageHandler<AuthorizationHandler>();

var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

builder.Services.AddAuthentication(oidcScheme)
                .AddKeycloakOpenIdConnect("keycloak", realm: "aspire", oidcScheme, options =>
                {
                    options.ClientId = "aspire-public-client";
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    //options.Scope.Add("weather:all");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    options.SaveTokens = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    
                    // เพิ่มการตั้งค่า logout
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProviderForSignOut = async context =>
                        {
                            // เพิ่ม id_token_hint สำหรับ logout
                            var idToken = await context.HttpContext.GetTokenAsync("id_token");
                            if (!string.IsNullOrEmpty(idToken))
                            {
                                context.ProtocolMessage.IdTokenHint = idToken;
                            }
                            
                            // ตั้งค่า post logout redirect uri
                            context.ProtocolMessage.PostLogoutRedirectUri = context.Request.Scheme + "://" + context.Request.Host + "/signout-callback-oidc";
                        }
                    };
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

// Add Hybrid Authentication State Provider
builder.Services.AddScoped<HybridAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<HybridAuthenticationStateProvider>());

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapLoginAndLogout();

app.Run();

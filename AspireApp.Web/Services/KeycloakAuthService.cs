using System.Text;
using System.Text.Json;

namespace AspireApp.Web.Services
{
    public interface IKeycloakAuthService
    {
        Task<AuthResult> LoginAsync(string username, string password);
    }

    public class KeycloakAuthService : IKeycloakAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            try
            {
                // Keycloak token endpoint
                var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/realms/{_configuration["Keycloak:Realm"]}/protocol/openid-connect/token";
                
                var formParams = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "password"),
                    new("client_id", _configuration["Keycloak:ClientId"] ?? "aspire-public-client"),
                    new("username", username),
                    new("password", password),
                    new("scope", "openid profile email")
                };

                var formContent = new FormUrlEncodedContent(formParams);
                var response = await _httpClient.PostAsync(tokenEndpoint, formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    return new AuthResult
                    {
                        IsSuccess = true,
                        AccessToken = tokenResponse?.AccessToken,
                        RefreshToken = tokenResponse?.RefreshToken,
                        ExpiresIn = tokenResponse?.ExpiresIn ?? 0
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Authentication failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? TokenType { get; set; }
    }
}
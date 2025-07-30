using System.Net.Http.Json;
using System.Net;

namespace AspireApp.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]?> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;
        try
        {
            await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("/weatherforecast", cancellationToken))
            {
                if (forecasts?.Count >= maxItems)
                {
                    break;
                }
                if (forecast is not null)
                {
                    forecasts ??= [];
                    forecasts.Add(forecast);
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Throw custom exception for 401
            throw new UnauthorizedAccessException();
        }
        return forecasts?.ToArray() ?? [];
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

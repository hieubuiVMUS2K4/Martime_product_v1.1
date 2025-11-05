using System.Text.Json;

namespace MaritimeEdge.Services;

/// <summary>
/// Interface for SignalK HTTP Client Service
/// </summary>
public interface ISignalKHttpClient
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class;
    Task<string> GetRawAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP Client service for SignalK API
/// Centralizes all SignalK HTTP communication
/// Uses IHttpClientFactory for proper HttpClient management
/// </summary>
public class SignalKHttpClient : ISignalKHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SignalKHttpClient> _logger;
    private readonly string _baseUrl;

    public SignalKHttpClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SignalKHttpClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("SignalK");
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("SignalK:BaseUrl", 
            "https://demo.signalk.org/signalk/v1/api") ?? string.Empty;
    }

    /// <summary>
    /// Get data from SignalK endpoint and deserialize to type T
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SignalK endpoint {Endpoint} returned {StatusCode}", 
                    endpoint, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error accessing SignalK endpoint: {Endpoint}", endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data from SignalK endpoint: {Endpoint}", endpoint);
            return null;
        }
    }

    /// <summary>
    /// Get raw JSON string from SignalK endpoint
    /// </summary>
    public async Task<string> GetRawAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SignalK endpoint {Endpoint} returned {StatusCode}", 
                    endpoint, response.StatusCode);
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raw data from SignalK endpoint: {Endpoint}", endpoint);
            return string.Empty;
        }
    }

    /// <summary>
    /// Check if SignalK server is accessible
    /// </summary>
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_baseUrl, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

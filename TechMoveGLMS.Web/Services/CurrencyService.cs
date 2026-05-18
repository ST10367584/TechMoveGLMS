using System.Text.Json;

namespace TechMoveGLMS.Web.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyService> _logger;
    private static decimal _cachedRate = 0;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private const decimal FallbackRate = 18.50m;

    public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
            return _cachedRate;

        await _lock.WaitAsync();
        try
        {
            if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
                return _cachedRate;

            var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                    rates.TryGetProperty("ZAR", out var zar))
                {
                    _cachedRate = zar.GetDecimal();
                    _cacheExpiry = DateTime.UtcNow.AddHours(1);
                    _logger.LogInformation("Rate updated: {Rate}", _cachedRate);
                    return _cachedRate;
                }
            }
            _logger.LogWarning("Using fallback rate {Rate}", FallbackRate);
            return FallbackRate;
        }
        finally { _lock.Release(); }
    }

    public decimal ConvertUsdToZar(decimal amountUsd, decimal rate)
    {
        if (rate <= 0) throw new ArgumentException("Rate must be positive", nameof(rate));
        if (amountUsd < 0) throw new ArgumentException("Amount cannot be negative", nameof(amountUsd));
        return Math.Round(amountUsd * rate, 2);
    }
}
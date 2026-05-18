using System.Text.Json;

namespace TechMoveGLMS.Web.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        decimal ConvertUsdToZar(decimal amountUsd, decimal rate);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        // Cache the rate for 1 hour to avoid hammering the free API
        private static decimal _cachedRate = 0;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        // Fallback rate if API fails
        private const decimal FallbackRate = 18.50m;

        public CurrencyService(HttpClient httpClient,
                               ILogger<CurrencyService> logger,
                               IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached rate if still valid
            if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
                return _cachedRate;

            await _lock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
                    return _cachedRate;

                // Use ExchangeRate-API (free tier, no key needed for latest endpoint)
                // Free API: https://open.er-api.com/v6/latest/USD
                var response = await _httpClient.GetAsync(
                    "https://open.er-api.com/v6/latest/USD");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                        rates.TryGetProperty("ZAR", out var zarElement))
                    {
                        _cachedRate = zarElement.GetDecimal();
                        _cacheExpiry = DateTime.UtcNow.AddHours(1);
                        _logger.LogInformation("Fetched USD→ZAR rate: {Rate}", _cachedRate);
                        return _cachedRate;
                    }
                }

                _logger.LogWarning("Currency API unavailable. Using fallback rate {Rate}", FallbackRate);
                return FallbackRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate. Using fallback {Rate}", FallbackRate);
                return FallbackRate;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Core business logic: Convert USD to ZAR using a given rate.
        /// This method is intentionally pure (no I/O) so it can be unit-tested easily.
        /// </summary>
        public decimal ConvertUsdToZar(decimal amountUsd, decimal rate)
        {
            if (rate <= 0)
                throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
            if (amountUsd < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amountUsd));

            return Math.Round(amountUsd * rate, 2);
        }
    }
}

using System.Text.Json;
namespace TechMoveGLMS.API.Services
{
    public class ExchangeRateApiConverter : ICurrencyConverter
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateApiConverter> _logger;
        private const string BaseUrl = "https://api.exchangerate-api.com/v4/latest/USD";

        public ExchangeRateApiConverter(HttpClient httpClient, ILogger<ExchangeRateApiConverter> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var rateData = JsonSerializer.Deserialize<ExchangeRateResponse>(content);

                if (rateData?.Rates != null && rateData.Rates.ContainsKey(toCurrency))
                {
                    return rateData.Rates[toCurrency];
                }

                return 19.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get exchange rate");
                return 19.50m;
            }
        }

        public decimal Convert(decimal amount, decimal rate)
        {
            return amount * rate;
        }
    }

    public class ExchangeRateResponse
    {
        public Dictionary<string, decimal> Rates { get; set; }
    }

}

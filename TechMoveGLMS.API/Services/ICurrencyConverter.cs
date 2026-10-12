namespace TechMoveGLMS.API.Services
{
    public interface ICurrencyConverter
    {
        Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency);
        decimal Convert(decimal amount, decimal rate);
    }
}

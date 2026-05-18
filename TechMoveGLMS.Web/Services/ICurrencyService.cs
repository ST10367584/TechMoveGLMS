namespace TechMoveGLMS.Web.Services;

public interface ICurrencyService
{
    Task<decimal> GetUsdToZarRateAsync();
    decimal ConvertUsdToZar(decimal amountUsd, decimal rate);
}
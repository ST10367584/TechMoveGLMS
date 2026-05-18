using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests;

public class CurrencyCalculationTests
{
    private readonly CurrencyService _service;
    public CurrencyCalculationTests()
    {
        var http = new HttpClient();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CurrencyService>.Instance;
        _service = new CurrencyService(http, logger);
    }

    [Fact]
    public void ConvertUsdToZar_100UsdAt18_5_Returns1850() =>
        Assert.Equal(1850m, _service.ConvertUsdToZar(100m, 18.5m));

    [Fact]
    public void ConvertUsdToZar_ZeroAmount_ReturnsZero() =>
        Assert.Equal(0m, _service.ConvertUsdToZar(0m, 18.5m));

    [Fact]
    public void ConvertUsdToZar_RoundsToTwoDecimals() =>
        Assert.Equal(18.33m, _service.ConvertUsdToZar(1m, 18.3333333m));

    [Fact]
    public void ConvertUsdToZar_NegativeAmount_Throws() =>
        Assert.Throws<ArgumentException>(() => _service.ConvertUsdToZar(-10m, 18m));

    [Fact]
    public void ConvertUsdToZar_ZeroRate_Throws() =>
        Assert.Throws<ArgumentException>(() => _service.ConvertUsdToZar(100m, 0m));
}
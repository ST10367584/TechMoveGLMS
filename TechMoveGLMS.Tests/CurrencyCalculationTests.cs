using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    /// <summary>
    /// Unit tests for CurrencyService business logic.
    /// These tests verify that the USD → ZAR conversion math is correct
    /// for any given rate — without making any network calls.
    /// </summary>
    public class CurrencyCalculationTests
    {
        // ── Helper: build a real CurrencyService with a mock HttpClient ──────
        private static CurrencyService BuildService()
        {
            var httpClient = new HttpClient(); // Won't be called — only ConvertUsdToZar is tested
            var logger = NullLogger<CurrencyService>.Instance;
            var config = new Mock<IConfiguration>().Object;
            return new CurrencyService(httpClient, logger, config);
        }

        // ── 1. Standard conversion ────────────────────────────────────────────
        [Fact]
        public void ConvertUsdToZar_CorrectResult_ForStandardRate()
        {
            // Arrange
            var service = BuildService();
            decimal amountUsd = 100m;
            decimal rate = 18.50m;

            // Act
            decimal result = service.ConvertUsdToZar(amountUsd, rate);

            // Assert
            Assert.Equal(1850.00m, result);
        }

        // ── 2. Zero USD input gives zero ZAR ──────────────────────────────────
        [Fact]
        public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
        {
            var service = BuildService();
            decimal result = service.ConvertUsdToZar(0m, 18.50m);
            Assert.Equal(0m, result);
        }

        // ── 3. Result is rounded to 2 decimal places ──────────────────────────
        [Fact]
        public void ConvertUsdToZar_RoundsToTwoDecimalPlaces()
        {
            var service = BuildService();
            // 1 USD * 18.333333... = 18.33 (rounded)
            decimal result = service.ConvertUsdToZar(1m, 18.3333333m);
            Assert.Equal(18.33m, result);
        }

        // ── 4. Fractional USD amounts ─────────────────────────────────────────
        [Fact]
        public void ConvertUsdToZar_FractionalAmount_CorrectResult()
        {
            var service = BuildService();
            // $250.50 * R19.25 = R4822.125 → rounded to R4822.13
            decimal result = service.ConvertUsdToZar(250.50m, 19.25m);
            Assert.Equal(4822.13m, result);
        }

        // ── 5. Large amounts ─────────────────────────────────────────────────
        [Fact]
        public void ConvertUsdToZar_LargeAmount_CorrectResult()
        {
            var service = BuildService();
            decimal result = service.ConvertUsdToZar(1_000_000m, 18.00m);
            Assert.Equal(18_000_000.00m, result);
        }

        // ── 6. Invalid rate (zero) throws ArgumentException ───────────────────
        [Fact]
        public void ConvertUsdToZar_ZeroRate_ThrowsArgumentException()
        {
            var service = BuildService();
            Assert.Throws<ArgumentException>(() => service.ConvertUsdToZar(100m, 0m));
        }

        // ── 7. Negative rate throws ArgumentException ─────────────────────────
        [Fact]
        public void ConvertUsdToZar_NegativeRate_ThrowsArgumentException()
        {
            var service = BuildService();
            Assert.Throws<ArgumentException>(() => service.ConvertUsdToZar(100m, -5m));
        }

        // ── 8. Negative USD amount throws ArgumentException ───────────────────
        [Fact]
        public void ConvertUsdToZar_NegativeAmount_ThrowsArgumentException()
        {
            var service = BuildService();
            Assert.Throws<ArgumentException>(() => service.ConvertUsdToZar(-50m, 18m));
        }

        // ── 9. Parametrized multi-rate test ──────────────────────────────────
        [Theory]
        [InlineData(10.00, 15.00, 150.00)]
        [InlineData(200.00, 18.50, 3700.00)]
        [InlineData(1.00, 20.00, 20.00)]
        [InlineData(0.50, 18.00, 9.00)]
        public void ConvertUsdToZar_MultipleRates_AllCorrect(
            double usd, double rate, double expectedZar)
        {
            var service = BuildService();
            decimal result = service.ConvertUsdToZar((decimal)usd, (decimal)rate);
            Assert.Equal((decimal)expectedZar, result);
        }
    }
}

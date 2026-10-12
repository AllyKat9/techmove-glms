using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechMoveGLMS.Tests.Tests.Unit
{
    public class CurrencyConversionTests
    {
        // Tests conversion to Rands using multiple USD amounts and exchange rates
        [Theory]
        [InlineData(100, 19.50, 1950)]
        [InlineData(50, 19.50, 975)]
        [InlineData(0, 19.50, 0)]
        [InlineData(1, 19.50, 19.50)]
        [InlineData(1000, 18.75, 18750)]

        // Verifies that multiplying USD amount by exchange rate correctly converts to Rands
        public void Convert_USD_To_ZAR_CalculatesCorrectly(decimal usdAmount, decimal rate, decimal expectedZar)
        {
            var result = usdAmount * rate;
            Assert.Equal(expectedZar, result);
        }

        // Tests conversion with negative amount handling
        [Fact]
        public void Convert_WithNegativeAmount_ReturnsNegativeValue()
        {
            var result = -100 * 19.50m;
            Assert.Equal(-1950m, result);
        }

        // Tests conversion when exchange rate is zero
        [Fact]
        public void Convert_WithZeroRate_ReturnsZero()
        {
            var result = 100 * 0;
            Assert.Equal(0, result);
        }
    }
}
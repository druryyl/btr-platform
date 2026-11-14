using BtrGudang.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace BtrGudang.Test.Helpers
{
    public class BtradeCloudOptionsTest
    {
        [Fact]
        public void BaseUrl_ShouldDefaultToHttpsCloudEndpoint()
        {
            var sut = new BtradeCloudOptions();

            sut.BaseUrl.Should().Be("https://dev.smart-ics.com:8089/belajar-api");
        }

        [Fact]
        public void GetEffectiveBaseUrl_ShouldReturnConfiguredValueWithoutTrailingSlash()
        {
            var sut = new BtradeCloudOptions { BaseUrl = "  https://example.com/belajar-api/  " };

            sut.GetEffectiveBaseUrl().Should().Be("https://example.com/belajar-api");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetEffectiveBaseUrl_ShouldFallBackToDefault_WhenBlank(string value)
        {
            var sut = new BtradeCloudOptions { BaseUrl = value };

            sut.GetEffectiveBaseUrl().Should().Be(BtradeCloudOptions.DEFAULT_BASE_URL);
        }
    }
}

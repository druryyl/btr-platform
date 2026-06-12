using btr.application.Portal;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.Portal
{
    public class PresentationModeServiceTest
    {
        [Fact]
        public void IsEnabled_WhenDisabled_ReturnsFalse()
        {
            var service = new PresentationModeService(
                Options.Create(new PresentationOptions { Enabled = false }));

            service.IsEnabled.Should().BeFalse();
        }

        [Fact]
        public void IsEnabled_WhenEnabled_ReturnsTrue()
        {
            var service = new PresentationModeService(
                Options.Create(new PresentationOptions { Enabled = true }));

            service.IsEnabled.Should().BeTrue();
        }

        [Fact]
        public void IsEnabled_WhenOptionsNull_ReturnsFalse()
        {
            var service = new PresentationModeService(null);

            service.IsEnabled.Should().BeFalse();
        }
    }
}

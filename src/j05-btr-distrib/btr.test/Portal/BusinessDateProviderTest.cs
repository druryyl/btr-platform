using System;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.Portal
{
    public class BusinessDateProviderTest
    {
        private static readonly DateTime SystemNow = new DateTime(2026, 8, 12, 14, 30, 0);

        [Fact]
        public void Today_WhenPresentationDisabled_ReturnsSystemDate()
        {
            var provider = CreateProvider(
                new PresentationOptions { Enabled = false },
                SystemNow);

            provider.Today.Should().Be(SystemNow.Date);
            provider.IsPresentationActive.Should().BeFalse();
        }

        [Fact]
        public void Today_WhenPresentationEnabled_ReturnsConfiguredBusinessDate()
        {
            var provider = CreateProvider(
                new PresentationOptions { Enabled = true, BusinessDate = "2026-06-05" },
                SystemNow);

            provider.Today.Should().Be(new DateTime(2026, 6, 5));
            provider.IsPresentationActive.Should().BeTrue();
        }

        [Fact]
        public void Today_WhenPresentationEnabledWithoutBusinessDate_Throws()
        {
            var provider = CreateProvider(
                new PresentationOptions { Enabled = true },
                SystemNow);

            Action act = () => { var _ = provider.Today; };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*BusinessDate*");
        }

        [Fact]
        public void Today_WhenPresentationEnabledWithInvalidBusinessDate_Throws()
        {
            var provider = CreateProvider(
                new PresentationOptions { Enabled = true, BusinessDate = "05-06-2026" },
                SystemNow);

            Action act = () => { var _ = provider.Today; };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*invalid*");
        }

        private static PresentationBusinessDateProvider CreateProvider(
            PresentationOptions options,
            DateTime systemNow)
        {
            return new PresentationBusinessDateProvider(
                Options.Create(options),
                new StubTglJamDal(systemNow));
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }
    }
}

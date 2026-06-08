using System;
using btr.application.ReportingContext.Shared;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class ReportPeriodValidatorTest
    {
        private static readonly DateTime ReferenceDate = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void ResolveAndValidate_DefaultsToCurrentMonth_WhenDatesOmitted()
        {
            var periode = ReportPeriodValidator.ResolveAndValidate(null, ReferenceDate);

            periode.Tgl1.Should().Be(new DateTime(2026, 6, 1));
            periode.Tgl2.Should().Be(new DateTime(2026, 6, 30, 23, 59, 59));
        }

        [Fact]
        public void ResolveAndValidate_AcceptsExplicitRange_WithinMaxDays()
        {
            var request = new ReportPeriodRequest
            {
                From = new DateTime(2026, 6, 1),
                To = new DateTime(2026, 6, 8),
            };

            var periode = ReportPeriodValidator.ResolveAndValidate(request, ReferenceDate);

            periode.Tgl1.Should().Be(new DateTime(2026, 6, 1));
            periode.Tgl2.Should().Be(new DateTime(2026, 6, 8, 23, 59, 59));
        }

        [Fact]
        public void ResolveAndValidate_RejectsRange_WhenFromAfterTo()
        {
            var request = new ReportPeriodRequest
            {
                From = new DateTime(2026, 6, 10),
                To = new DateTime(2026, 6, 1),
            };

            Action act = () => ReportPeriodValidator.ResolveAndValidate(request, ReferenceDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*from*");
        }

        [Fact]
        public void ResolveAndValidate_RejectsRange_WhenExceedsMaxDays()
        {
            var request = new ReportPeriodRequest
            {
                From = new DateTime(2026, 6, 1),
                To = new DateTime(2026, 7, 2),
            };

            Action act = () => ReportPeriodValidator.ResolveAndValidate(request, ReferenceDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage($"*{ReportPeriodValidator.MaxDays}*");
        }
    }
}

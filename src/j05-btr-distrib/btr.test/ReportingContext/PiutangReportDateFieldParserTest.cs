using System;
using btr.application.ReportingContext.PiutangReportAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PiutangReportDateFieldParserTest
    {
        [Theory]
        [InlineData(null, PiutangReportDateField.DueDate)]
        [InlineData("", PiutangReportDateField.DueDate)]
        [InlineData("DueDate", PiutangReportDateField.DueDate)]
        [InlineData("duedate", PiutangReportDateField.DueDate)]
        [InlineData("PiutangDate", PiutangReportDateField.PiutangDate)]
        public void Parse_AcceptsValidValues(string value, PiutangReportDateField expected)
        {
            PiutangReportDateFieldParser.Parse(value).Should().Be(expected);
        }

        [Fact]
        public void Parse_RejectsInvalidValue()
        {
            Action act = () => PiutangReportDateFieldParser.Parse("FakturDate");

            act.Should().Throw<ArgumentException>();
        }
    }
}

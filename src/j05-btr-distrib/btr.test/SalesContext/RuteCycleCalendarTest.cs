using System;
using System.Collections.Generic;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SupportContext.ParamSistemAgg;
using btr.domain.SupportContext.ParamSistemAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class RuteCycleCalendarTest
    {
        private readonly RuteCycleCalendar _calendar = new RuteCycleCalendar(new StubParamSistemDal());

        [Fact]
        public void ResolveHariRuteId_AnchorMonday_ReturnsH11()
        {
            _calendar.ResolveHariRuteId(new DateTime(2026, 1, 5)).Should().Be("H11");
        }

        [Fact]
        public void ResolveHariRuteId_Sunday_ReturnsNull()
        {
            _calendar.ResolveHariRuteId(new DateTime(2026, 1, 11)).Should().BeNull();
        }

        [Fact]
        public void ResolveHariRuteId_SecondWeekMonday_ReturnsH21()
        {
            _calendar.ResolveHariRuteId(new DateTime(2026, 1, 12)).Should().Be("H21");
        }

        [Fact]
        public void ResolveHariRuteId_CycleWrapAfter14Days_ReturnsH11()
        {
            _calendar.ResolveHariRuteId(new DateTime(2026, 1, 19)).Should().Be("H11");
        }

        [Fact]
        public void ResolveHariRuteId_ThursdaySecondWeek_ReturnsH24()
        {
            _calendar.ResolveHariRuteId(new DateTime(2026, 1, 15)).Should().Be("H24");
        }

        [Fact]
        public void ResolveHariRuteId_YearBoundary_NoReset()
        {
            _calendar.ResolveHariRuteId(new DateTime(2027, 1, 4)).Should().Be("H11");
        }

        [Fact]
        public void GetCycleWeekLabel_ReturnsMinggu1And2()
        {
            _calendar.GetCycleWeekLabel(new DateTime(2026, 1, 5)).Should().Be("Minggu1");
            _calendar.GetCycleWeekLabel(new DateTime(2026, 1, 12)).Should().Be("Minggu2");
        }

        private sealed class StubParamSistemDal : IParamSistemDal
        {
            public void Insert(ParamSistemModel model) { }

            public void Update(ParamSistemModel model) { }

            public void Delete(ParamSistemModel model) { }

            public ParamSistemModel GetData(IParamSistemKey key)
            {
                if (key.ParamCode == RuteCycleCalendar.AnchorDateParamCode)
                {
                    return new ParamSistemModel(key.ParamCode) { ParamValue = "2026-01-05" };
                }

                return null;
            }

            public IEnumerable<ParamSistemModel> ListData()
            {
                return new List<ParamSistemModel>();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesPersonAgg;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.application.SupportContext.ParamSistemAgg;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.domain.SupportContext.ParamSistemAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class RegenerateVisitPlanWorkerTest
    {
        private static readonly DateTime Today = new DateTime(2026, 6, 9);

        [Fact]
        public void Execute_MapsTemplateCustomersForResolvedDate()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var sut = CreateWorker(
                visitPlanDal,
                CreateTemplate("SP1",
                    new SalesRuteModel { SalesRuteId = "RT1", SalesPersonId = "SP1", HariRuteId = "H12" },
                    new SalesRuteItemModel { SalesRuteId = "RT1", CustomerId = "C001", NoUrut = 1 },
                    new SalesRuteItemModel { SalesRuteId = "RT1", CustomerId = "C002", NoUrut = 2 }));

            sut.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = "SP1",
                FromDate = Today,
                ToDate = Today,
                TriggeredBy = "Test"
            });

            visitPlanDal.InsertedRows.Should().HaveCount(2);
            visitPlanDal.InsertedRows.Should().OnlyContain(x =>
                x.SalesPersonId == "SP1" &&
                x.VisitDate == Today &&
                x.HariRuteId == "H12" &&
                x.PlanSource == "Template");
            visitPlanDal.InsertedRows.Select(x => x.CustomerId).Should().BeEquivalentTo("C001", "C002");
        }

        [Fact]
        public void Execute_SkipsSunday()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var sunday = new DateTime(2026, 6, 14);
            var sut = CreateWorker(
                visitPlanDal,
                CreateTemplate("SP1",
                    new SalesRuteModel { SalesRuteId = "RT1", SalesPersonId = "SP1", HariRuteId = "H16" },
                    new SalesRuteItemModel { SalesRuteId = "RT1", CustomerId = "C001", NoUrut = 1 }));

            sut.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = "SP1",
                FromDate = sunday,
                ToDate = sunday,
                TriggeredBy = "Test"
            });

            visitPlanDal.InsertedRows.Should().BeEmpty();
            visitPlanDal.InsertedRows.Should().NotContain(x => x.VisitDate.DayOfWeek == DayOfWeek.Sunday);
        }

        [Fact]
        public void Execute_EmptyTemplateSlot_ProducesNoRowsForDate()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var wednesday = new DateTime(2026, 6, 10);
            var sut = CreateWorker(
                visitPlanDal,
                CreateTemplate("SP1",
                    new SalesRuteModel { SalesRuteId = "RT1", SalesPersonId = "SP1", HariRuteId = "H12" },
                    new SalesRuteItemModel { SalesRuteId = "RT1", CustomerId = "C001", NoUrut = 1 }));

            sut.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = "SP1",
                FromDate = wednesday,
                ToDate = wednesday,
                TriggeredBy = "Test"
            });

            visitPlanDal.InsertedRows.Should().BeEmpty();
        }

        [Fact]
        public void Execute_PastFromDate_DeletesOnlyFromToday()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var pastDate = Today.AddDays(-3);
            var sut = CreateWorker(
                visitPlanDal,
                CreateTemplate("SP1",
                    new SalesRuteModel { SalesRuteId = "RT1", SalesPersonId = "SP1", HariRuteId = "H12" },
                    new SalesRuteItemModel { SalesRuteId = "RT1", CustomerId = "C001", NoUrut = 1 }));

            sut.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = "SP1",
                FromDate = pastDate,
                ToDate = Today,
                TriggeredBy = "Test"
            });

            visitPlanDal.DeleteCalls.Should().ContainSingle();
            visitPlanDal.DeleteCalls[0].FromDate.Should().Be(Today);
            visitPlanDal.DeleteCalls[0].ToDate.Should().Be(Today);
        }

        private static RegenerateVisitPlanWorker CreateWorker(
            RecordingVisitPlanDal visitPlanDal,
            (StubSalesRuteDal Routes, StubSalesRuteItemDal Items) template)
        {
            return new RegenerateVisitPlanWorker(
                visitPlanDal,
                template.Routes,
                template.Items,
                new RuteCycleCalendar(new StubParamSistemDal()),
                new StubParamSistemDal(),
                new StubTglJamDal(Today));
        }

        private static (StubSalesRuteDal Routes, StubSalesRuteItemDal Items) CreateTemplate(
            string salesPersonId,
            SalesRuteModel route,
            params SalesRuteItemModel[] items)
        {
            route.SalesPersonId = salesPersonId;
            var routes = new StubSalesRuteDal(new[] { route });
            var routeItems = items.Select(x =>
            {
                x.SalesRuteId = route.SalesRuteId;
                return x;
            }).ToList();
            var itemsDal = new StubSalesRuteItemDal(routeItems);
            return (routes, itemsDal);
        }

        private sealed class RecordingVisitPlanDal : IVisitPlanDal
        {
            public List<VisitPlanModel> InsertedRows { get; } = new List<VisitPlanModel>();
            public List<DeleteCall> DeleteCalls { get; } = new List<DeleteCall>();

            public IEnumerable<VisitPlanModel> ListData(IVisitPlanDateKey filter) =>
                Enumerable.Empty<VisitPlanModel>();

            public IEnumerable<VisitPlanModel> ListData(VisitPlanDateRangeFilter filter) =>
                Enumerable.Empty<VisitPlanModel>();

            public IEnumerable<string> ListSalesPersonIdsWithRoutes() =>
                Enumerable.Empty<string>();

            public void DeleteFuture(string salesPersonId, DateTime fromDate, DateTime toDate)
            {
                DeleteCalls.Add(new DeleteCall(salesPersonId, fromDate, toDate));
            }

            public void BulkInsert(IEnumerable<VisitPlanModel> rows)
            {
                InsertedRows.AddRange(rows);
            }
        }

        private sealed class DeleteCall
        {
            public DeleteCall(string salesPersonId, DateTime fromDate, DateTime toDate)
            {
                SalesPersonId = salesPersonId;
                FromDate = fromDate;
                ToDate = toDate;
            }

            public string SalesPersonId { get; }
            public DateTime FromDate { get; }
            public DateTime ToDate { get; }
        }

        private sealed class StubSalesRuteDal : ISalesRuteDal
        {
            private readonly List<SalesRuteModel> _routes;

            public StubSalesRuteDal(IEnumerable<SalesRuteModel> routes)
            {
                _routes = routes.ToList();
            }

            public void Insert(SalesRuteModel model) => throw new NotSupportedException();
            public void Update(SalesRuteModel model) => throw new NotSupportedException();
            public void Delete(ISalesRuteKey key) => throw new NotSupportedException();
            public SalesRuteModel GetData(ISalesRuteKey key) => null;

            public IEnumerable<SalesRuteModel> ListData(ISalesPersonKey filter) =>
                _routes.Where(x => x.SalesPersonId == filter.SalesPersonId);
        }

        private sealed class StubSalesRuteItemDal : ISalesRuteItemDal
        {
            private readonly List<SalesRuteItemModel> _items;

            public StubSalesRuteItemDal(IEnumerable<SalesRuteItemModel> items)
            {
                _items = items.ToList();
            }

            public void Insert(IEnumerable<SalesRuteItemModel> listModel) => throw new NotSupportedException();
            public void Delete(ISalesRuteKey key) => throw new NotSupportedException();

            public IEnumerable<SalesRuteItemModel> ListData(ISalesRuteKey filter) =>
                _items.Where(x => x.SalesRuteId == filter.SalesRuteId);
        }

        private sealed class StubParamSistemDal : IParamSistemDal
        {
            public void Insert(ParamSistemModel model) { }
            public void Update(ParamSistemModel model) { }
            public void Delete(ParamSistemModel model) { }

            public ParamSistemModel GetData(IParamSistemKey key)
            {
                if (key.ParamCode == RuteCycleCalendar.AnchorDateParamCode)
                    return new ParamSistemModel(key.ParamCode) { ParamValue = "2026-01-05" };

                if (key.ParamCode == RegenerateVisitPlanWorker.HorizonDaysParamCode)
                    return new ParamSistemModel(key.ParamCode) { ParamValue = "90" };

                return null;
            }

            public IEnumerable<ParamSistemModel> ListData() => Enumerable.Empty<ParamSistemModel>();
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now) => Now = now;
            public DateTime Now { get; }
        }
    }
}

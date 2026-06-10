using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesPersonAgg;
using btr.application.SalesContext.SalesRuteAgg;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.application.SupportContext.ParamSistemAgg;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.domain.SupportContext.ParamSistemAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesRuteVisitPlanMaterializationTest
    {
        private static readonly DateTime Today = new DateTime(2026, 6, 9);

        [Fact]
        [Trait("Category", "Integration")]
        public void TemplateSave_RegeneratesFutureRowsMatchingTemplate()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var salesRuteDal = new StubSalesRuteDal();
            var salesRuteItemDal = new StubSalesRuteItemDal();
            var regenerateWorker = new RegenerateVisitPlanWorker(
                visitPlanDal,
                salesRuteDal,
                salesRuteItemDal,
                new RuteCycleCalendar(new StubParamSistemDal()),
                new StubParamSistemDal(),
                new StubTglJamDal(Today));

            var writer = new SalesRuteWriter(
                salesRuteDal,
                salesRuteItemDal,
                new StubCounter(),
                regenerateWorker,
                new StubTglJamDal(Today));

            var model = new SalesRuteModel
            {
                SalesPersonId = "SP1",
                HariRuteId = "H12",
                ListCustomer = new List<SalesRuteItemModel>
                {
                    new SalesRuteItemModel { CustomerId = "C001", NoUrut = 1 },
                    new SalesRuteItemModel { CustomerId = "C002", NoUrut = 2 }
                }
            };

            writer.Save(model);

            visitPlanDal.DeleteCalls.Should().ContainSingle();
            visitPlanDal.DeleteCalls[0].FromDate.Should().Be(Today);
            visitPlanDal.DeleteCalls[0].SalesPersonId.Should().Be("SP1");

            var todayRows = visitPlanDal.InsertedRows
                .Where(x => x.VisitDate == Today)
                .ToList();
            todayRows.Should().HaveCount(2);
            todayRows.Select(x => x.CustomerId).Should().BeEquivalentTo("C001", "C002");
            todayRows.Should().OnlyContain(x => x.HariRuteId == "H12" && x.PlanSource == "Template");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TemplateSave_WithPastFromDateRequest_DoesNotDeleteBeforeToday()
        {
            var visitPlanDal = new RecordingVisitPlanDal();
            var salesRuteDal = new StubSalesRuteDal();
            var salesRuteItemDal = new StubSalesRuteItemDal();
            var regenerateWorker = new RegenerateVisitPlanWorker(
                visitPlanDal,
                salesRuteDal,
                salesRuteItemDal,
                new RuteCycleCalendar(new StubParamSistemDal()),
                new StubParamSistemDal(),
                new StubTglJamDal(Today));

            regenerateWorker.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = "SP1",
                FromDate = Today.AddDays(-7),
                ToDate = Today,
                TriggeredBy = "TemplateSave"
            });

            visitPlanDal.DeleteCalls.Should().ContainSingle();
            visitPlanDal.DeleteCalls[0].FromDate.Should().BeOnOrAfter(Today);
            visitPlanDal.DeleteCalls[0].FromDate.Should().Be(Today);
        }

        private sealed class RecordingVisitPlanDal : IVisitPlanDal
        {
            public List<VisitPlanModel> InsertedRows { get; } = new List<VisitPlanModel>();
            public List<(string SalesPersonId, DateTime FromDate, DateTime ToDate)> DeleteCalls { get; } =
                new List<(string, DateTime, DateTime)>();

            public IEnumerable<VisitPlanModel> ListData(IVisitPlanDateKey filter) =>
                Enumerable.Empty<VisitPlanModel>();

            public IEnumerable<VisitPlanModel> ListData(VisitPlanDateRangeFilter filter) =>
                Enumerable.Empty<VisitPlanModel>();

            public IEnumerable<string> ListSalesPersonIdsWithRoutes() =>
                new[] { "SP1" };

            public void DeleteFuture(string salesPersonId, DateTime fromDate, DateTime toDate)
            {
                DeleteCalls.Add((salesPersonId, fromDate, toDate));
            }

            public void BulkInsert(IEnumerable<VisitPlanModel> rows)
            {
                InsertedRows.AddRange(rows);
            }
        }

        private sealed class StubSalesRuteDal : ISalesRuteDal
        {
            private readonly List<SalesRuteModel> _routes = new List<SalesRuteModel>();

            public void Insert(SalesRuteModel model) => _routes.Add(model);
            public void Update(SalesRuteModel model) { }
            public void Delete(ISalesRuteKey key) => throw new NotSupportedException();

            public SalesRuteModel GetData(ISalesRuteKey key) => null;

            public IEnumerable<SalesRuteModel> ListData(ISalesPersonKey filter) =>
                _routes.Where(x => x.SalesPersonId == filter.SalesPersonId);
        }

        private sealed class StubSalesRuteItemDal : ISalesRuteItemDal
        {
            private readonly List<SalesRuteItemModel> _items = new List<SalesRuteItemModel>();

            public void Insert(IEnumerable<SalesRuteItemModel> listModel) =>
                _items.AddRange(listModel);

            public void Delete(ISalesRuteKey key) =>
                _items.RemoveAll(x => x.SalesRuteId == key.SalesRuteId);

            public IEnumerable<SalesRuteItemModel> ListData(ISalesRuteKey filter) =>
                _items.Where(x => x.SalesRuteId == filter.SalesRuteId);
        }

        private sealed class StubCounter : INunaCounterBL
        {
            public string Generate(string anchor, int length) => "RT001";
            public string Generate(string anchor, string prefix, int length) => "RT001";
            public string Generate(string prefix, IDFormatEnum format) => "RT001";
            public string Generate(string prefix, IDFormatEnum format, string sourceFlag) => "RT001";
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

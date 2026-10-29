using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.distrib.SalesContext.VisitPlanAgg;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace btr.test.SalesContext
{
    public class JadwalKunjunganFormTest
    {
        [Fact]
        public void Constructor_WithFullDependencyGraph_InstantiatesSuccessfully()
        {
            var salesPersonDal = new StubSalesPersonDal();
            var customerDal = new StubCustomerDal();
            var visitPlanDal = new StubVisitPlanDal();
            var exceptionDal = new StubVisitPlanExceptionDal();
            var effectiveVisitPlanDal = new StubEffectiveVisitPlanDal();
            var exceptionWriter = new StubVisitPlanExceptionWriter();
            var calendar = new StubRuteCycleCalendar();
            var regenerateWorker = new StubRegenerateVisitPlanWorker();

            using (var form = new JadwalKunjunganForm(
                salesPersonDal,
                customerDal,
                visitPlanDal,
                exceptionDal,
                effectiveVisitPlanDal,
                exceptionWriter,
                calendar,
                regenerateWorker))
            {
                form.Should().NotBeNull();

                var workerField = typeof(JadwalKunjunganForm).GetField("_regenerateWorker", BindingFlags.NonPublic | BindingFlags.Instance);
                workerField.Should().NotBeNull();
                workerField.GetValue(form).Should().BeSameAs(regenerateWorker);

                var materializeControls = form.Controls.Find("MaterializeButton", true);
                materializeControls.Should().ContainSingle();
                var materializeButton = materializeControls[0].As<Button>();
                materializeButton.Text.Should().Be("Materialisasi");
                materializeButton.Enabled.Should().BeTrue();
                materializeButton.Parent.Should().NotBeNull();
                materializeButton.Parent.Name.Should().Be("TopPanel");
            }
        }

        [Fact]
        public void DependencyResolution_FromServiceProvider_ResolvesJadwalKunjunganForm()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ISalesPersonDal>(new StubSalesPersonDal());
            services.AddSingleton<ICustomerDal>(new StubCustomerDal());
            services.AddSingleton<IVisitPlanDal>(new StubVisitPlanDal());
            services.AddSingleton<IVisitPlanExceptionDal>(new StubVisitPlanExceptionDal());
            services.AddSingleton<IEffectiveVisitPlanDal>(new StubEffectiveVisitPlanDal());
            services.AddSingleton<IVisitPlanExceptionWriter>(new StubVisitPlanExceptionWriter());
            services.AddSingleton<IRuteCycleCalendar>(new StubRuteCycleCalendar());
            services.AddSingleton<IRegenerateVisitPlanWorker>(new StubRegenerateVisitPlanWorker());

            services.AddTransient<JadwalKunjunganForm>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var form = serviceProvider.GetRequiredService<JadwalKunjunganForm>())
                {
                    form.Should().NotBeNull();

                    var workerField = typeof(JadwalKunjunganForm).GetField("_regenerateWorker", BindingFlags.NonPublic | BindingFlags.Instance);
                    workerField.Should().NotBeNull();
                    workerField.GetValue(form).Should().NotBeNull();
                    workerField.GetValue(form).Should().BeAssignableTo<IRegenerateVisitPlanWorker>();
                }
            }
        }

        private sealed class StubSalesPersonDal : ISalesPersonDal
        {
            private readonly List<SalesPersonModel> _list;

            public StubSalesPersonDal(params SalesPersonModel[] list)
            {
                _list = list.ToList();
            }

            public IEnumerable<SalesPersonModel> ListData() => _list;
            public void Insert(SalesPersonModel model) => throw new NotSupportedException();
            public void Update(SalesPersonModel model) => throw new NotSupportedException();
            public void Delete(ISalesPersonKey key) => throw new NotSupportedException();
            public SalesPersonModel GetData(ISalesPersonKey key) => _list.FirstOrDefault(x => x.SalesPersonId == key.SalesPersonId);
        }

        private sealed class StubCustomerDal : ICustomerDal
        {
            public void Insert(CustomerModel model) => throw new NotSupportedException();
            public void Update(CustomerModel model) => throw new NotSupportedException();
            public void Delete(ICustomerKey key) => throw new NotSupportedException();
            public CustomerModel GetData(ICustomerKey key) => null;
            public IEnumerable<CustomerModel> ListData() => Enumerable.Empty<CustomerModel>();
            public IEnumerable<CustomerLocationView> ListLocation() => Enumerable.Empty<CustomerLocationView>();
        }

        private sealed class StubVisitPlanDal : IVisitPlanDal
        {
            public IEnumerable<VisitPlanModel> ListData(IVisitPlanDateKey filter) => Enumerable.Empty<VisitPlanModel>();
            public IEnumerable<VisitPlanModel> ListData(VisitPlanDateRangeFilter filter) => Enumerable.Empty<VisitPlanModel>();
            public IEnumerable<string> ListSalesPersonIdsWithRoutes() => Enumerable.Empty<string>();
            public void DeleteFuture(string salesPersonId, DateTime fromDate, DateTime toDate) { }
            public void DeleteRange(string salesPersonId, DateTime fromDate, DateTime toDate) { }
            public void Insert(IEnumerable<VisitPlanModel> list) { }
            public void BulkInsert(IEnumerable<VisitPlanModel> list) { }
        }

        private sealed class StubVisitPlanExceptionDal : IVisitPlanExceptionDal
        {
            public IEnumerable<VisitPlanExceptionModel> ListData(IVisitPlanDateKey filter) => Enumerable.Empty<VisitPlanExceptionModel>();
            public IEnumerable<VisitPlanExceptionModel> ListData(VisitPlanDateRangeFilter filter) => Enumerable.Empty<VisitPlanExceptionModel>();
            public void Delete(IVisitPlanExceptionKey key) { }
            public void DeleteByDate(string salesPersonId, DateTime visitDate) { }
            public void Insert(VisitPlanExceptionModel model) { }
            public VisitPlanExceptionModel GetData(IVisitPlanExceptionKey key) => null;
        }

        private sealed class StubEffectiveVisitPlanDal : IEffectiveVisitPlanDal
        {
            public IEnumerable<EffectiveVisitPlanEntry> ListEffectivePlan(string salesPersonId, DateTime visitDate) =>
                Enumerable.Empty<EffectiveVisitPlanEntry>();
        }

        private sealed class StubVisitPlanExceptionWriter : IVisitPlanExceptionWriter
        {
            public VisitPlanExceptionModel Save(VisitPlanExceptionModel model) => model;
            public void Delete(IVisitPlanExceptionKey key) { }
        }

        private sealed class StubRuteCycleCalendar : IRuteCycleCalendar
        {
            public string ResolveHariRuteId(DateTime visitDate) => "H12";
            public string GetCycleWeekLabel(DateTime visitDate) => "Minggu 1";
            public int GetCycleWeekNumber(DateTime visitDate) => 1;
            public DateTime GetAnchorDate() => DateTime.Today;
        }

        private sealed class StubRegenerateVisitPlanWorker : IRegenerateVisitPlanWorker
        {
            public List<RegenerateVisitPlanRequest> Requests { get; } = new List<RegenerateVisitPlanRequest>();

            public void Execute(RegenerateVisitPlanRequest request)
            {
                Requests.Add(request);
            }
        }
    }
}

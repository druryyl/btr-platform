using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class FieldActivityComposerTest
    {
        private readonly DateTime _visitDate = new DateTime(2026, 6, 10);
        private readonly FieldActivityOptions _options = new FieldActivityOptions
        {
            VisitPlanGoLiveDate = new DateTime(2026, 3, 1)
        };

        [Fact]
        public void Compose_WhenAllPlannedVisited_Execution100AndNoMissed()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1), Plan("C2", 2) },
                checkIns: new[] { CheckIn("C1", "08:00:00"), CheckIn("C2", "09:00:00") });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.PlannedVisits.Should().Be(2);
            result.Kpis.ActualVisits.Should().Be(2);
            result.Kpis.MissedVisits.Should().Be(0);
            result.Kpis.VisitExecutionPercent.Should().Be(100);
            result.MissedVisits.Should().BeEmpty();
        }

        [Fact]
        public void Compose_WhenPartialVisits_ReportsMissedCustomers()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1), Plan("C2", 2), Plan("C3", 3) },
                checkIns: new[] { CheckIn("C1", "08:00:00") });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.MissedVisits.Should().Be(2);
            result.MissedVisits.Select(x => x.CustomerId).Should().BeEquivalentTo(new[] { "C2", "C3" });
        }

        [Fact]
        public void Compose_WhenUnplannedCheckIn_IncrementsUnplannedCount()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: new[] { CheckIn("C1", "08:00:00"), CheckIn("C9", "10:00:00") });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.UnplannedVisits.Should().Be(1);
            result.ActualStops.Single(x => x.CustomerId == "C9").VisitStatus.Should().Be("Unplanned");
        }

        [Fact]
        public void Compose_WhenOrderOnSameDate_MarksEffectiveCall()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: new[] { CheckIn("C1", "08:00:00") },
                orderCustomers: new[] { "C1" });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.EffectiveCalls.Should().Be(1);
            result.ActualStops.Single().IsEffectiveCall.Should().BeTrue();
        }

        [Fact]
        public void Compose_WhenOrderWithoutCheckIn_DoesNotCountEffectiveCall()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: Array.Empty<FieldActivityCheckInRow>(),
                orderCustomers: new[] { "C1" });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.EffectiveCalls.Should().Be(0);
            result.Kpis.ActualVisits.Should().Be(0);
        }

        [Fact]
        public void Compose_WhenPlannedZero_VisitExecutionPercentIsNull()
        {
            var fixture = CreateFixture(
                checkIns: new[] { CheckIn("C9", "08:00:00") });

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.PlannedVisits.Should().Be(0);
            result.Kpis.VisitExecutionPercent.Should().BeNull();
        }

        [Fact]
        public void Compose_WhenActualZero_EffectiveCallRateIsNull()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: Array.Empty<FieldActivityCheckInRow>());

            var result = fixture.Compose("SP1", _visitDate);

            result.Kpis.ActualVisits.Should().Be(0);
            result.Kpis.EffectiveCallRate.Should().BeNull();
        }

        [Fact]
        public void Compose_WhenZeroCoordinates_ExcludesFromGeometry()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: new[]
                {
                    new FieldActivityCheckInRow
                    {
                        CustomerId = "C1",
                        CustomerCode = "C1",
                        CustomerName = "Customer 1",
                        CheckInTime = "08:00:00",
                        CheckInLatitude = 0,
                        CheckInLongitude = 0,
                        CustomerLatitude = 0,
                        CustomerLongitude = 0
                    }
                },
                coordinates: new Dictionary<string, CustomerCoordinateRow>
                {
                    ["C1"] = new CustomerCoordinateRow { CustomerId = "C1", Latitude = 0, Longitude = 0 }
                });

            var result = fixture.Compose("SP1", _visitDate);

            result.PlannedStops.Single().HasCoordinates.Should().BeFalse();
            result.ActualStops.Single().HasCoordinates.Should().BeFalse();
            result.RouteGeometry.Planned.Coordinates.Should().BeEmpty();
            result.RouteGeometry.Actual.Coordinates.Should().BeEmpty();
        }

        [Fact]
        public void Compose_WhenBeforeGoLiveDate_PlannedSetEmptyAndMetaFalse()
        {
            var fixture = CreateFixture(
                plan: new[] { Plan("C1", 1) },
                checkIns: new[] { CheckIn("C1", "08:00:00") });

            var result = fixture.Compose("SP1", new DateTime(2026, 1, 15));

            result.Kpis.PlannedVisits.Should().Be(0);
            result.Meta.PlanDataAvailable.Should().BeFalse();
            result.Kpis.ActualVisits.Should().Be(1);
        }

        [Fact]
        public void Compose_WhenSalesmanHasNoEmail_ThrowsArgumentException()
        {
            var fixture = CreateFixture(email: string.Empty);

            Action act = () => fixture.Compose("SP1", _visitDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Email*");
        }

        private static EffectiveVisitPlanEntry Plan(string customerId, int noUrut)
        {
            return new EffectiveVisitPlanEntry
            {
                CustomerId = customerId,
                CustomerCode = customerId,
                CustomerName = $"Customer {customerId}",
                NoUrut = noUrut
            };
        }

        private static FieldActivityCheckInRow CheckIn(string customerId, string time)
        {
            return new FieldActivityCheckInRow
            {
                CustomerId = customerId,
                CustomerCode = customerId,
                CustomerName = $"Customer {customerId}",
                CheckInTime = time,
                CheckInLatitude = -6.2,
                CheckInLongitude = 106.8,
                CustomerLatitude = -6.2,
                CustomerLongitude = 106.8,
                Accuracy = 10
            };
        }

        private FieldActivityComposer CreateFixture(
            EffectiveVisitPlanEntry[] plan = null,
            FieldActivityCheckInRow[] checkIns = null,
            string[] orderCustomers = null,
            IDictionary<string, CustomerCoordinateRow> coordinates = null,
            string email = "rep@example.com")
        {
            var planList = (plan ?? Array.Empty<EffectiveVisitPlanEntry>()).ToList();
            var checkInList = (checkIns ?? Array.Empty<FieldActivityCheckInRow>()).ToList();
            var orderSet = new HashSet<string>(orderCustomers ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            var coordDict = coordinates ?? planList.ToDictionary(
                x => x.CustomerId,
                x => new CustomerCoordinateRow
                {
                    CustomerId = x.CustomerId,
                    Latitude = -6.2,
                    Longitude = 106.8
                });

            return new FieldActivityComposer(
                new StubSalesPersonDal(email),
                new StubEffectiveVisitPlanDal(planList),
                new StubCheckInDal(checkInList),
                new StubOrderDal(orderSet),
                new StubCoordinateDal(coordDict),
                _options);
        }

        private sealed class StubSalesPersonDal : ISalesPersonDal
        {
            private readonly string _email;

            public StubSalesPersonDal(string email) => _email = email;

            public void Insert(SalesPersonModel model) { }
            public void Update(SalesPersonModel model) { }
            public void Delete(ISalesPersonKey key) { }

            public SalesPersonModel GetData(ISalesPersonKey key)
            {
                return new SalesPersonModel
                {
                    SalesPersonId = key.SalesPersonId,
                    SalesPersonName = "Test Rep",
                    Email = _email
                };
            }

            public IEnumerable<SalesPersonModel> ListData()
            {
                yield return GetData(new SalesPersonModel("SP1"));
            }
        }

        private sealed class StubEffectiveVisitPlanDal : IEffectiveVisitPlanDal
        {
            private readonly IList<EffectiveVisitPlanEntry> _plan;

            public StubEffectiveVisitPlanDal(IList<EffectiveVisitPlanEntry> plan) => _plan = plan;

            public IEnumerable<EffectiveVisitPlanEntry> ListEffectivePlan(string salesPersonId, DateTime visitDate)
            {
                return _plan;
            }
        }

        private sealed class StubCheckInDal : IFieldActivityCheckInDal
        {
            private readonly IList<FieldActivityCheckInRow> _rows;

            public StubCheckInDal(IList<FieldActivityCheckInRow> rows) => _rows = rows;

            public IReadOnlyList<FieldActivityCheckInRow> ListBySalesPersonDate(
                string salesPersonEmail, DateTime visitDate)
            {
                return _rows.ToList();
            }
        }

        private sealed class StubOrderDal : IFieldActivityOrderDal
        {
            private readonly ISet<string> _customerIds;

            public StubOrderDal(ISet<string> customerIds) => _customerIds = customerIds;

            public ISet<string> ListCustomerIdsWithOrder(string salesPersonEmail, DateTime visitDate)
            {
                return _customerIds;
            }
        }

        private sealed class StubCoordinateDal : ICustomerCoordinateDal
        {
            private readonly IDictionary<string, CustomerCoordinateRow> _coords;

            public StubCoordinateDal(IDictionary<string, CustomerCoordinateRow> coords) => _coords = coords;

            public IReadOnlyDictionary<string, CustomerCoordinateRow> ListByCustomerIds(
                IEnumerable<string> customerIds)
            {
                return customerIds
                    .Where(id => _coords.ContainsKey(id))
                    .ToDictionary(id => id, id => _coords[id]);
            }
        }
    }
}

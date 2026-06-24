using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityAttentionEngineTest
    {
        [Fact]
        public void DiffAndPersistSignals_NewSignal_CreatesLifecycleRow()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);
            var generatedAt = new DateTime(2026, 6, 24, 8, 0, 0);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001", Signal("C001", DashboardCustomerAggregator.SignalOverdue)),
                "r1",
                generatedAt);

            repository.AttentionRows.Should().ContainSingle();
            var row = repository.AttentionRows[0];
            row.SignalCode.Should().Be(DashboardCustomerAggregator.SignalOverdue);
            row.FirstSeenPeriodYear.Should().Be(2026);
            row.FirstSeenPeriodMonth.Should().Be(6);
            row.LastSeenPeriodYear.Should().Be(2026);
            row.LastSeenPeriodMonth.Should().Be(6);
            row.ConsecutivePeriods.Should().Be(1);
            row.TotalOccurrences.Should().Be(1);
            row.IsActive.Should().BeTrue();
        }

        [Fact]
        public void DiffAndPersistSignals_ExistingSignalSameMonth_IsIdempotent()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);
            var generatedAt = new DateTime(2026, 6, 24, 8, 0, 0);
            var signals = Signals("C001", Signal("C001", DashboardCustomerAggregator.SignalOverdue));

            engine.DiffAndPersistSignals(EntityTypeCode.Customer, 2026, 6, signals, "r1", generatedAt);
            engine.DiffAndPersistSignals(EntityTypeCode.Customer, 2026, 6, signals, "r2", generatedAt.AddHours(1));

            repository.AttentionRows.Should().ContainSingle();
            repository.AttentionRows[0].ConsecutivePeriods.Should().Be(1);
            repository.AttentionRows[0].TotalOccurrences.Should().Be(1);
            repository.AttentionRows[0].LastRefreshLogId.Should().Be("r2");
        }

        [Fact]
        public void DiffAndPersistSignals_ExistingSignalNextMonth_IncrementsStreak()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);
            var signal = Signal("C001", DashboardCustomerAggregator.SignalDormant);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                5,
                Signals("C001", signal),
                "r1",
                new DateTime(2026, 5, 31));

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001", signal),
                "r2",
                new DateTime(2026, 6, 24));

            var row = repository.AttentionRows.Single();
            row.ConsecutivePeriods.Should().Be(2);
            row.TotalOccurrences.Should().Be(2);
            row.LastSeenPeriodMonth.Should().Be(6);
            row.IsActive.Should().BeTrue();
        }

        [Fact]
        public void DiffAndPersistSignals_SignalResolved_MarksInactive()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);
            var signal = Signal("C001", DashboardCustomerAggregator.SignalPlafondBreach);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001", signal),
                "r1",
                new DateTime(2026, 6, 24));

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001"),
                "r2",
                new DateTime(2026, 6, 24, 12, 0, 0));

            var row = repository.AttentionRows.Single();
            row.IsActive.Should().BeFalse();
            row.LastSeenPeriodMonth.Should().Be(6);
            row.ConsecutivePeriods.Should().Be(1);
            row.TotalOccurrences.Should().Be(1);
        }

        [Fact]
        public void DiffAndPersistSignals_SignalReappearsLater_StartsNewStreak()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);
            var signal = Signal("C001", DashboardCustomerAggregator.SignalOverdue);

            engine.DiffAndPersistSignals(EntityTypeCode.Customer, 2026, 4, Signals("C001", signal), "r1", new DateTime(2026, 4, 30));
            engine.DiffAndPersistSignals(EntityTypeCode.Customer, 2026, 5, Signals("C001"), "r2", new DateTime(2026, 5, 31));
            engine.DiffAndPersistSignals(EntityTypeCode.Customer, 2026, 6, Signals("C001", signal), "r3", new DateTime(2026, 6, 24));

            var row = repository.AttentionRows.Single();
            row.IsActive.Should().BeTrue();
            row.FirstSeenPeriodMonth.Should().Be(4);
            row.LastSeenPeriodMonth.Should().Be(6);
            row.ConsecutivePeriods.Should().Be(1);
            row.TotalOccurrences.Should().Be(2);
        }

        [Fact]
        public void DiffAndPersistSignals_MultipleSignalsOnSameEntity_PersistsEach()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals(
                    "C001",
                    Signal("C001", DashboardCustomerAggregator.SignalOverdue),
                    Signal("C001", DashboardCustomerAggregator.SignalDormant)),
                "r1",
                new DateTime(2026, 6, 24));

            repository.AttentionRows.Should().HaveCount(2);
            repository.AttentionRows.Select(r => r.SignalCode).Should().BeEquivalentTo(
                DashboardCustomerAggregator.SignalOverdue,
                DashboardCustomerAggregator.SignalDormant);
        }

        [Fact]
        public void DiffAndPersistSignals_MultipleEntityTypes_Isolated()
        {
            var repository = new AttentionRepository();
            var engine = CreateEngine(repository);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001", Signal("C001", DashboardCustomerAggregator.SignalOverdue)),
                "r1",
                new DateTime(2026, 6, 24));

            engine.DiffAndPersistSignals(
                EntityTypeCode.Salesman,
                2026,
                6,
                Signals("S001", new EntityAttentionSignalSnapshot
                {
                    EntityId = "S001",
                    EntityCode = "S001",
                    SignalCode = "LowOmzet",
                    SignalCategory = "Performance",
                    SignalTitle = "Low Omzet"
                }),
                "r1",
                new DateTime(2026, 6, 24));

            repository.AttentionRows.Should().HaveCount(2);
            repository.AttentionRows.Should().Contain(r => r.EntityType == EntityTypeCode.Customer);
            repository.AttentionRows.Should().Contain(r => r.EntityType == EntityTypeCode.Salesman);
        }

        [Fact]
        public void BuildAttentionSection_ReturnsActiveAndHistoricalCounts()
        {
            var repository = new AttentionRepository();
            repository.AttentionRows.AddRange(new[]
            {
                AttentionRow("C001", DashboardCustomerAggregator.SignalOverdue, true, 2026, 4, 2026, 6, 3, 3),
                AttentionRow("C001", DashboardCustomerAggregator.SignalDormant, false, 2026, 1, 2026, 3, 3, 3)
            });

            var section = CreateEngine(repository).BuildAttentionSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeTrue();
            section.ActiveSignalCount.Should().Be(1);
            section.HistoricalSignalCount.Should().Be(1);
            section.Events.Should().HaveCount(2);
            section.Events[0].IsActive.Should().BeTrue();
            section.Events[0].FirstSeen.Should().Be("Apr 2026");
            section.Events[0].ConsecutivePeriods.Should().Be(3);
            section.Events[0].TotalOccurrences.Should().Be(3);
        }

        [Fact]
        public void DiffAndPersistSignals_ClosedMonth_SkipsWrite()
        {
            var repository = new AttentionRepository();
            repository.CloseMonth(EntityTypeCode.Customer, 2026, 6, "close");
            var engine = CreateEngine(repository);

            engine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                2026,
                6,
                Signals("C001", Signal("C001", DashboardCustomerAggregator.SignalOverdue)),
                "r1",
                new DateTime(2026, 6, 24));

            repository.AttentionRows.Should().BeEmpty();
        }

        private static EntityAttentionEngine CreateEngine(AttentionRepository repository)
        {
            return new EntityAttentionEngine(repository);
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>> Signals(
            string entityId,
            params EntityAttentionSignalSnapshot[] signals)
        {
            return new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>
            {
                [entityId] = signals?.ToList() ?? new List<EntityAttentionSignalSnapshot>()
            };
        }

        private static EntityAttentionSignalSnapshot Signal(string entityId, string signalCode)
        {
            return new EntityAttentionSignalSnapshot
            {
                EntityId = entityId,
                EntityCode = entityId,
                SignalCode = signalCode,
                SignalCategory = "Finance",
                SignalTitle = signalCode
            };
        }

        private static EntityAnalyticsAttentionEventRow AttentionRow(
            string entityId,
            string signalCode,
            bool isActive,
            int firstYear,
            int firstMonth,
            int lastYear,
            int lastMonth,
            int consecutive,
            int total)
        {
            return new EntityAnalyticsAttentionEventRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                SignalCode = signalCode,
                SignalTitle = signalCode,
                SignalCategory = "Finance",
                FirstSeenPeriodYear = firstYear,
                FirstSeenPeriodMonth = firstMonth,
                LastSeenPeriodYear = lastYear,
                LastSeenPeriodMonth = lastMonth,
                ConsecutivePeriods = consecutive,
                TotalOccurrences = total,
                IsActive = isActive,
                GeneratedAt = new DateTime(lastYear, lastMonth, 1)
            };
        }

        private sealed class AttentionRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => Array.Empty<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SupplierEntityAnalyticsProducer : IEntityAnalyticsProducer
    {
        public string EntityType => EntityTypeCode.Supplier;

        public string WorkerDomain => "PurchasingManagement";

        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityAnalyticsMonthCloseService _monthCloseService;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityRadarEngine _radarEngine;
        private readonly IAttentionSignalRegistry _attentionSignals;
        private readonly IPrincipalSalesOutSnapshotDal _salesOutSnapshotDal;
        private readonly IPrincipalReturnSnapshotDal _returnSnapshotDal;
        private readonly IPrincipalReturnPercentageSnapshotDal _returnPercentageSnapshotDal;
        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;
        private readonly IPrincipalAchievementSnapshotDal _achievementSnapshotDal;

        public SupplierEntityAnalyticsProducer(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityAnalyticsMonthCloseService monthCloseService,
            IEntityRankingEngine rankingEngine,
            IEntityAttentionEngine attentionEngine,
            IEntityRelationshipEngine relationshipEngine,
            IEntityRadarEngine radarEngine,
            IAttentionSignalRegistry attentionSignals,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal = null,
            IPrincipalReturnSnapshotDal returnSnapshotDal = null,
            IPrincipalReturnPercentageSnapshotDal returnPercentageSnapshotDal = null,
            IPrincipalTargetSnapshotDal targetSnapshotDal = null,
            IPrincipalAchievementSnapshotDal achievementSnapshotDal = null)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _monthCloseService = monthCloseService;
            _rankingEngine = rankingEngine;
            _attentionEngine = attentionEngine;
            _relationshipEngine = relationshipEngine;
            _radarEngine = radarEngine;
            _attentionSignals = attentionSignals;
            _salesOutSnapshotDal = salesOutSnapshotDal;
            _returnSnapshotDal = returnSnapshotDal;
            _returnPercentageSnapshotDal = returnPercentageSnapshotDal;
            _targetSnapshotDal = targetSnapshotDal;
            _achievementSnapshotDal = achievementSnapshotDal;
        }

        public void Produce(EntityAnalyticsProduceContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.DomainInput as SupplierEntityAnalyticsProduceInput;
            var portfolio = input?.ManagementAggregate?.Portfolio;
            var (periodYear, periodMonth) = EntityAnalyticsProducerReplaySupport.ResolvePeriod(context);
            var salesOut = ReadOwnedSalesOut(periodYear, periodMonth);
            var returns = ReadOwnedReturns(periodYear, periodMonth);
            var returnPercentage = ReadOwnedReturnPercentage(periodYear, periodMonth);
            var target = ReadOwnedTarget(periodYear, periodMonth);
            var achievement = ReadOwnedAchievement(periodYear, periodMonth);

            if (portfolio == null || portfolio.Count == 0)
            {
                var retainedRows = new List<EntityAnalyticsCurrentRow>();
                var retainedMonthly = new List<EntityAnalyticsMonthlyRow>();
                if (salesOut.MatchesPeriod)
                {
                    AppendSnapshotOnlySalesOut(
                        retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, salesOut);
                }
                else
                {
                    RetainPersistedSalesOut(retainedRows, context.GeneratedAt, salesOut);
                }

                AppendSnapshotOnlyReturns(
                    retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, returns);
                RetainPersistedReturns(retainedRows, context.GeneratedAt, returns);
                AppendSnapshotOnlyReturnPercentage(
                    retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, returnPercentage);
                RetainPersistedReturnPercentage(retainedRows, context.GeneratedAt, returnPercentage);
                AppendSnapshotOnlyTarget(
                    retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, target);
                RetainPersistedTarget(retainedRows, context.GeneratedAt, target);
                AppendSnapshotOnlyAchievement(
                    retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, achievement);
                RetainPersistedAchievement(retainedRows, context.GeneratedAt, achievement);

                EntityAnalyticsProducerReplaySupport.PersistL0(
                    _repository, context, EntityTypeCode.Supplier, retainedRows);
                if (retainedMonthly.Count > 0)
                {
                    EntityAnalyticsProducerReplaySupport.PersistL1(
                        _repository, _monthCloseService, context, EntityTypeCode.Supplier, retainedMonthly);
                    _rankingEngine.ComputeAndPersistRanks(
                        EntityTypeCode.Supplier,
                        periodYear,
                        periodMonth,
                        context.RefreshLogId,
                        context.GeneratedAt,
                        context.Replay);
                }

                return;
            }

            var attentionById = BuildAttentionIndex(input.ManagementAggregate);
            var attentionDetailById = BuildAttentionDetailIndex(input.ManagementAggregate);
            var rows = new List<EntityAnalyticsCurrentRow>();
            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();
            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var supplier in portfolio)
            {
                if (string.IsNullOrWhiteSpace(supplier.SupplierId))
                    continue;

                var entityId = supplier.SupplierId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(supplier.SupplierCode)
                    ? entityId
                    : supplier.SupplierCode.Trim();
                var generatedAt = context.GeneratedAt;

                rows.AddRange(BuildSupplierRows(supplier, entityId, entityCode, generatedAt, attentionById));
                monthlyRows.AddRange(BuildMonthlyRows(supplier, entityId, entityCode, periodYear, periodMonth, generatedAt));
                ComposeSalesOut(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, salesOut);
                ComposeReturns(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, returns);
                ComposeReturnPercentage(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, returnPercentage);
                ComposeTarget(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, target);
                ComposeAchievement(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, achievement);
                signalsByEntity[entityId] = BuildAttentionSnapshots(
                    entityId,
                    entityCode,
                    attentionById,
                    attentionDetailById);
            }

            AppendSnapshotOnlySalesOut(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, salesOut);
            RetainPersistedSalesOut(rows, context.GeneratedAt, salesOut);
            AppendSnapshotOnlyReturns(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, returns);
            RetainPersistedReturns(rows, context.GeneratedAt, returns);
            AppendSnapshotOnlyReturnPercentage(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, returnPercentage);
            RetainPersistedReturnPercentage(rows, context.GeneratedAt, returnPercentage);
            AppendSnapshotOnlyTarget(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, target);
            RetainPersistedTarget(rows, context.GeneratedAt, target);
            AppendSnapshotOnlyAchievement(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, achievement);
            RetainPersistedAchievement(rows, context.GeneratedAt, achievement);

            EntityAnalyticsProducerReplaySupport.PersistL0(_repository, context, EntityTypeCode.Supplier, rows);

            EntityAnalyticsProducerReplaySupport.PersistL1(
                _repository, _monthCloseService, context, EntityTypeCode.Supplier, monthlyRows);

            _rankingEngine.ComputeAndPersistRanks(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _attentionEngine.DiffAndPersistSignals(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                signalsByEntity,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _relationshipEngine.PersistRollups(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                BuildRelationshipSnapshots(input, context),
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _radarEngine.ComputeAndPersistScores(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);
        }

        private SalesOutComposition ReadOwnedSalesOut(int periodYear, int periodMonth)
        {
            var existing = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.SalesOutId);

            var snapshot = _salesOutSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && string.Equals(snapshot.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.OrdinalIgnoreCase)
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalSalesOutRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalSalesOutRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    if (!string.IsNullOrWhiteSpace(row.KpiId)
                        && !string.Equals(row.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new SalesOutComposition(matchesPeriod, bySupplierId, existing);
        }

        private void ComposeSalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (salesOut.MatchesPeriod && salesOut.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddSalesOutRows(rows, monthlyRows, entityId, entityCode, owned.SalesOutAmount, periodYear, periodMonth, generatedAt);
                return;
            }

            if (salesOut.MatchesPeriod)
                return;

            var persisted = salesOut.Existing.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            if (persisted?.NumericValue == null)
                return;

            AddSalesOutRows(rows, null, entityId, entityCode, persisted.NumericValue.Value, periodYear, periodMonth, generatedAt);
        }

        private void AppendSnapshotOnlySalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (!salesOut.MatchesPeriod)
                return;

            foreach (var owned in salesOut.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsSalesOut(rows, entityId))
                    continue;

                var entityCode = entityId;
                var displayName = string.IsNullOrWhiteSpace(owned.SupplierName) ? entityId : owned.SupplierName.Trim();
                rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, displayName, generatedAt));
                rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, 1m, null, generatedAt));
                AddSalesOutRows(rows, monthlyRows, entityId, entityCode, owned.SalesOutAmount, periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedSalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (salesOut.MatchesPeriod)
                return;

            foreach (var persisted in salesOut.Existing)
            {
                if (string.IsNullOrWhiteSpace(persisted.EntityId) || persisted.NumericValue == null)
                    continue;

                var entityId = persisted.EntityId.Trim();
                if (ContainsSalesOut(rows, entityId))
                    continue;

                var entityCode = string.IsNullOrWhiteSpace(persisted.EntityCode) ? entityId : persisted.EntityCode.Trim();
                CopyIdentity(rows, entityId, entityCode, generatedAt);
                AddSalesOutRows(rows, null, entityId, entityCode, persisted.NumericValue.Value, 0, 0, generatedAt);
            }
        }

        private ReturnComposition ReadOwnedReturns(int periodYear, int periodMonth)
        {
            var existingGood = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.GoodReturnAmountId);
            var existingBroken = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.BrokenReturnAmountId);
            var existingTotal = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.TotalReturnAmountId);

            var snapshot = _returnSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalReturnRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalReturnRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new ReturnComposition(matchesPeriod, bySupplierId, existingGood, existingBroken, existingTotal);
        }

        private ReturnPercentageComposition ReadOwnedReturnPercentage(int periodYear, int periodMonth)
        {
            var existing = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.ReturnPercentageId);

            var snapshot = _returnPercentageSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && string.Equals(snapshot.ReturnPercentageKpiId, PrincipalKpiCatalog.ReturnPercentageId, StringComparison.OrdinalIgnoreCase)
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalReturnPercentageRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalReturnPercentageRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new ReturnPercentageComposition(matchesPeriod, bySupplierId, existing);
        }

        private void ComposeReturns(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            ReturnComposition returns)
        {
            if (returns.MatchesPeriod && returns.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddReturnRows(rows, monthlyRows, entityId, entityCode, owned, periodYear, periodMonth, generatedAt);
                return;
            }

            if (returns.MatchesPeriod)
                return;

            var good = returns.ExistingGood.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            var broken = returns.ExistingBroken.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            var total = returns.ExistingTotal.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));

            if (good?.NumericValue == null && broken?.NumericValue == null && total?.NumericValue == null)
                return;

            AddReturnRows(rows, null, entityId, entityCode,
                good?.NumericValue, broken?.NumericValue, total?.NumericValue,
                0, 0, generatedAt);
        }

        private void ComposeReturnPercentage(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            ReturnPercentageComposition percentage)
        {
            if (percentage.MatchesPeriod && percentage.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddReturnPercentageRows(rows, monthlyRows, entityId, entityCode, owned.ReturnPercentage, periodYear, periodMonth, generatedAt);
                return;
            }

            if (percentage.MatchesPeriod)
                return;

            var persisted = percentage.Existing.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            if (persisted == null)
                return;

            AddReturnPercentageRows(rows, null, entityId, entityCode, persisted.NumericValue, 0, 0, generatedAt);
        }

        private void AppendSnapshotOnlyReturns(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            ReturnComposition returns)
        {
            if (!returns.MatchesPeriod)
                return;

            foreach (var owned in returns.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.GoodReturnAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.BrokenReturnAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.TotalReturnAmountId))
                    continue;

                EnsureIdentity(rows, entityId, entityId, owned.SupplierName, generatedAt);
                AddReturnRows(rows, monthlyRows, entityId, entityId, owned, periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedReturns(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            ReturnComposition returns)
        {
            if (returns.MatchesPeriod)
                return;

            var entityIds = returns.ExistingGood
                .Concat(returns.ExistingBroken)
                .Concat(returns.ExistingTotal)
                .Where(row => !string.IsNullOrWhiteSpace(row.EntityId))
                .Select(row => row.EntityId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var entityId in entityIds)
            {
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.GoodReturnAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.BrokenReturnAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.TotalReturnAmountId))
                    continue;

                var good = returns.ExistingGood.FirstOrDefault(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
                var broken = returns.ExistingBroken.FirstOrDefault(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
                var total = returns.ExistingTotal.FirstOrDefault(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));

                if (good?.NumericValue == null && broken?.NumericValue == null && total?.NumericValue == null)
                    continue;

                var entityCode = good?.EntityCode ?? broken?.EntityCode ?? total?.EntityCode;
                if (string.IsNullOrWhiteSpace(entityCode))
                    entityCode = entityId;

                CopyIdentity(rows, entityId, entityCode.Trim(), generatedAt);
                AddReturnRows(rows, null, entityId, entityCode.Trim(),
                    good?.NumericValue, broken?.NumericValue, total?.NumericValue,
                    0, 0, generatedAt);
            }
        }

        private void AppendSnapshotOnlyReturnPercentage(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            ReturnPercentageComposition percentage)
        {
            if (!percentage.MatchesPeriod)
                return;

            foreach (var owned in percentage.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.ReturnPercentageId))
                    continue;

                EnsureIdentity(rows, entityId, entityId, owned.SupplierName, generatedAt);
                AddReturnPercentageRows(rows, monthlyRows, entityId, entityId, owned.ReturnPercentage, periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedReturnPercentage(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            ReturnPercentageComposition percentage)
        {
            if (percentage.MatchesPeriod)
                return;

            foreach (var persisted in percentage.Existing)
            {
                if (string.IsNullOrWhiteSpace(persisted.EntityId))
                    continue;

                var entityId = persisted.EntityId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.ReturnPercentageId))
                    continue;

                var entityCode = string.IsNullOrWhiteSpace(persisted.EntityCode) ? entityId : persisted.EntityCode.Trim();
                CopyIdentity(rows, entityId, entityCode, generatedAt);
                AddReturnPercentageRows(rows, null, entityId, entityCode, persisted.NumericValue, 0, 0, generatedAt);
            }
        }

        private TargetComposition ReadOwnedTarget(int periodYear, int periodMonth)
        {
            var existing = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.TargetId);

            var snapshot = _targetSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && string.Equals(snapshot.KpiId, PrincipalKpiCatalog.TargetId, StringComparison.OrdinalIgnoreCase)
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalTargetRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new TargetComposition(matchesPeriod, bySupplierId, existing);
        }

        private AchievementComposition ReadOwnedAchievement(int periodYear, int periodMonth)
        {
            var existingAmount = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.AchievementAmountId);
            var existingPercentage = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.AchievementPercentageId);

            var snapshot = _achievementSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && string.Equals(snapshot.AchievementAmountKpiId, PrincipalKpiCatalog.AchievementAmountId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(snapshot.AchievementPercentageKpiId, PrincipalKpiCatalog.AchievementPercentageId, StringComparison.OrdinalIgnoreCase)
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalAchievementRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalAchievementRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new AchievementComposition(matchesPeriod, bySupplierId, existingAmount, existingPercentage);
        }

        private void ComposeTarget(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            TargetComposition target)
        {
            if (target.MatchesPeriod && target.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddTargetRows(rows, monthlyRows, entityId, entityCode, owned.TargetAmount, periodYear, periodMonth, generatedAt);
                return;
            }

            if (target.MatchesPeriod)
                return;

            var persisted = target.Existing.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            if (persisted?.NumericValue == null)
                return;

            AddTargetRows(rows, null, entityId, entityCode, persisted.NumericValue, 0, 0, generatedAt);
        }

        private void ComposeAchievement(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            AchievementComposition achievement)
        {
            if (achievement.MatchesPeriod && achievement.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddAchievementRows(rows, monthlyRows, entityId, entityCode,
                    owned.AchievementAmount, owned.AchievementPercentage,
                    periodYear, periodMonth, generatedAt);
                return;
            }

            if (achievement.MatchesPeriod)
                return;

            var amount = achievement.ExistingAmount.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            var percentage = achievement.ExistingPercentage.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));

            if (amount?.NumericValue == null && percentage?.NumericValue == null)
                return;

            AddAchievementRows(rows, null, entityId, entityCode,
                amount?.NumericValue, percentage?.NumericValue,
                0, 0, generatedAt);
        }

        private void AppendSnapshotOnlyTarget(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            TargetComposition target)
        {
            if (!target.MatchesPeriod)
                return;

            foreach (var owned in target.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.TargetId))
                    continue;

                EnsureIdentity(rows, entityId, entityId, owned.SupplierName, generatedAt);
                AddTargetRows(rows, monthlyRows, entityId, entityId, owned.TargetAmount, periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedTarget(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            TargetComposition target)
        {
            if (target.MatchesPeriod)
                return;

            foreach (var persisted in target.Existing)
            {
                if (string.IsNullOrWhiteSpace(persisted.EntityId) || persisted.NumericValue == null)
                    continue;

                var entityId = persisted.EntityId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.TargetId))
                    continue;

                var entityCode = string.IsNullOrWhiteSpace(persisted.EntityCode) ? entityId : persisted.EntityCode.Trim();
                CopyIdentity(rows, entityId, entityCode, generatedAt);
                AddTargetRows(rows, null, entityId, entityCode, persisted.NumericValue, 0, 0, generatedAt);
            }
        }

        private void AppendSnapshotOnlyAchievement(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            AchievementComposition achievement)
        {
            if (!achievement.MatchesPeriod)
                return;

            foreach (var owned in achievement.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.AchievementAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.AchievementPercentageId))
                    continue;

                EnsureIdentity(rows, entityId, entityId, owned.SupplierName, generatedAt);
                AddAchievementRows(rows, monthlyRows, entityId, entityId,
                    owned.AchievementAmount, owned.AchievementPercentage,
                    periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedAchievement(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            AchievementComposition achievement)
        {
            if (achievement.MatchesPeriod)
                return;

            var entityIds = achievement.ExistingAmount
                .Concat(achievement.ExistingPercentage)
                .Where(row => !string.IsNullOrWhiteSpace(row.EntityId))
                .Select(row => row.EntityId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var entityId in entityIds)
            {
                if (ContainsKpi(rows, entityId, PrincipalKpiCatalog.AchievementAmountId)
                    && ContainsKpi(rows, entityId, PrincipalKpiCatalog.AchievementPercentageId))
                    continue;

                var amount = achievement.ExistingAmount.FirstOrDefault(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
                var percentage = achievement.ExistingPercentage.FirstOrDefault(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));

                if (amount?.NumericValue == null && percentage?.NumericValue == null)
                    continue;

                var entityCode = amount?.EntityCode ?? percentage?.EntityCode;
                if (string.IsNullOrWhiteSpace(entityCode))
                    entityCode = entityId;

                CopyIdentity(rows, entityId, entityCode.Trim(), generatedAt);
                AddAchievementRows(rows, null, entityId, entityCode.Trim(),
                    amount?.NumericValue, percentage?.NumericValue,
                    0, 0, generatedAt);
            }
        }

        private void EnsureIdentity(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string displayName,
            DateTime generatedAt)
        {
            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.DisplayName,
                    null,
                    string.IsNullOrWhiteSpace(displayName) ? entityCode : displayName.Trim(),
                    generatedAt));
            }

            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, 1m, null, generatedAt));
            }
        }

        private void CopyIdentity(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            DateTime generatedAt)
        {
            var current = _repository.GetCurrentMetrics(EntityTypeCode.Supplier, entityId);
            var displayName = current.FirstOrDefault(row =>
                string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase));
            var isActive = current.FirstOrDefault(row =>
                string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase));

            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.DisplayName,
                    null,
                    displayName?.TextValue ?? entityCode,
                    generatedAt));
            }

            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.IsActive,
                    isActive?.NumericValue ?? 1m,
                    null,
                    generatedAt));
            }
        }

        private void AddSalesOutRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal amount,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            if (ContainsSalesOut(rows, entityId))
                return;

            rows.Add(CreateRow(entityId, entityCode, PrincipalKpiCatalog.SalesOutId, amount, null, generatedAt));

            if (monthlyRows == null || periodYear <= 0 || periodMonth <= 0)
                return;

            if (!_kpiRegistry.TryGetMetadata(PrincipalKpiCatalog.SalesOutId, out var metadata) || !metadata.TrendEligible)
                return;

            monthlyRows.Add(new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = entityId,
                EntityCode = entityCode,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                NumericValue = amount,
                PeriodSemantics = metadata.PeriodSemantics,
                DefinitionVersion = metadata.DefinitionVersion,
                IsClosed = false,
                GeneratedAt = generatedAt
            });
        }

        private static bool ContainsSalesOut(IEnumerable<EntityAnalyticsCurrentRow> rows, string entityId)
        {
            return ContainsKpi(rows, entityId, PrincipalKpiCatalog.SalesOutId);
        }

        private static bool ContainsKpi(IEnumerable<EntityAnalyticsCurrentRow> rows, string entityId, string kpiId)
        {
            return rows.Any(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.KpiId, kpiId, StringComparison.OrdinalIgnoreCase));
        }

        private void AddReturnRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            PrincipalReturnRow owned,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddReturnRows(rows, monthlyRows, entityId, entityCode,
                owned.GoodReturnAmount, owned.BrokenReturnAmount, owned.TotalReturnAmount,
                periodYear, periodMonth, generatedAt);
        }

        private void AddReturnRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal? goodReturnAmount,
            decimal? brokenReturnAmount,
            decimal? totalReturnAmount,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.GoodReturnAmountId, goodReturnAmount, periodYear, periodMonth, generatedAt);
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.BrokenReturnAmountId, brokenReturnAmount, periodYear, periodMonth, generatedAt);
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.TotalReturnAmountId, totalReturnAmount, periodYear, periodMonth, generatedAt);
        }

        private void AddReturnPercentageRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal? returnPercentage,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.ReturnPercentageId, returnPercentage, periodYear, periodMonth, generatedAt);
        }

        private void AddTargetRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal? targetAmount,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.TargetId, targetAmount, periodYear, periodMonth, generatedAt);
        }

        private void AddAchievementRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            PrincipalAchievementRow owned,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddAchievementRows(rows, monthlyRows, entityId, entityCode,
                owned.AchievementAmount, owned.AchievementPercentage,
                periodYear, periodMonth, generatedAt);
        }

        private void AddAchievementRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal? achievementAmount,
            decimal? achievementPercentage,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.AchievementAmountId, achievementAmount, periodYear, periodMonth, generatedAt);
            AddKpiRow(rows, monthlyRows, entityId, entityCode,
                PrincipalKpiCatalog.AchievementPercentageId, achievementPercentage, periodYear, periodMonth, generatedAt);
        }

        private void AddKpiRow(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            string kpiId,
            decimal? amount,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            if (ContainsKpi(rows, entityId, kpiId))
                return;

            rows.Add(CreateRow(entityId, entityCode, kpiId, amount, null, generatedAt));

            if (monthlyRows == null || periodYear <= 0 || periodMonth <= 0)
                return;

            if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)
                return;

            monthlyRows.Add(new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = entityId,
                EntityCode = entityCode,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                KpiId = kpiId,
                NumericValue = amount,
                PeriodSemantics = metadata.PeriodSemantics,
                DefinitionVersion = metadata.DefinitionVersion,
                IsClosed = false,
                GeneratedAt = generatedAt
            });
        }

        private sealed class SalesOutComposition
        {
            public SalesOutComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalSalesOutRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existing)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalSalesOutRow>(StringComparer.OrdinalIgnoreCase);
                Existing = existing ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalSalesOutRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> Existing { get; }
        }

        private sealed class ReturnComposition
        {
            public ReturnComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalReturnRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existingGood,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existingBroken,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existingTotal)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalReturnRow>(StringComparer.OrdinalIgnoreCase);
                ExistingGood = existingGood ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
                ExistingBroken = existingBroken ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
                ExistingTotal = existingTotal ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalReturnRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> ExistingGood { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> ExistingBroken { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> ExistingTotal { get; }
        }

        private sealed class ReturnPercentageComposition
        {
            public ReturnPercentageComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalReturnPercentageRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existing)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalReturnPercentageRow>(StringComparer.OrdinalIgnoreCase);
                Existing = existing ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalReturnPercentageRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> Existing { get; }
        }

        private sealed class TargetComposition
        {
            public TargetComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalTargetRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existing)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
                Existing = existing ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalTargetRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> Existing { get; }
        }

        private sealed class AchievementComposition
        {
            public AchievementComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalAchievementRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existingAmount,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existingPercentage)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalAchievementRow>(StringComparer.OrdinalIgnoreCase);
                ExistingAmount = existingAmount ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
                ExistingPercentage = existingPercentage ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalAchievementRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> ExistingAmount { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> ExistingPercentage { get; }
        }

        private IEnumerable<EntityAnalyticsMonthlyRow> BuildMonthlyRows(
            DashboardPurchasingManagementPortfolioRow supplier,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            foreach (var (kpiId, value) in BuildTrendKpiValues(supplier))
            {
                if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)
                    continue;

                yield return new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Supplier,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    KpiId = kpiId,
                    NumericValue = value,
                    PeriodSemantics = metadata.PeriodSemantics,
                    DefinitionVersion = metadata.DefinitionVersion,
                    IsClosed = false,
                    GeneratedAt = generatedAt
                };
            }
        }

        private static IEnumerable<(string KpiId, decimal? Value)> BuildTrendKpiValues(
            DashboardPurchasingManagementPortfolioRow supplier)
        {
            yield return ("PU-KPI-001", supplier.MtdPurchaseAmount);
            yield return ("PU-KPI-002", supplier.MtdInvoiceCount);
            yield return ("PU-KPI-003", supplier.PostedPercent);
        }

        private static Dictionary<string, HashSet<string>> BuildAttentionIndex(
            DashboardPurchasingManagementAggregateResult aggregate)
        {
            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (!string.Equals(
                        row.EntityType,
                        DashboardPurchasingManagementAggregator.EntityTypePrincipal,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.SupplierId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = row.SupplierId.Trim();
                if (!index.TryGetValue(key, out var signals))
                {
                    signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    index[key] = signals;
                }

                signals.Add(row.SignalKey);
            }

            return index;
        }

        private static Dictionary<string, DashboardPurchasingManagementAttentionRow> BuildAttentionDetailIndex(
            DashboardPurchasingManagementAggregateResult aggregate)
        {
            var index = new Dictionary<string, DashboardPurchasingManagementAttentionRow>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (string.IsNullOrWhiteSpace(row.SupplierId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = $"{row.SupplierId.Trim()}:{row.SignalKey.Trim()}";
                index[key] = row;
            }

            return index;
        }

        private IReadOnlyList<EntityAttentionSignalSnapshot> BuildAttentionSnapshots(
            string entityId,
            string entityCode,
            IReadOnlyDictionary<string, HashSet<string>> attentionById,
            IReadOnlyDictionary<string, DashboardPurchasingManagementAttentionRow> attentionDetailById)
        {
            if (!attentionById.TryGetValue(entityId, out var signalCodes) || signalCodes.Count == 0)
                return Array.Empty<EntityAttentionSignalSnapshot>();

            var snapshots = new List<EntityAttentionSignalSnapshot>();
            foreach (var signalCode in signalCodes.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                attentionDetailById.TryGetValue($"{entityId}:{signalCode}", out var detail);
                if (!_attentionSignals.TryResolve(EntityType, signalCode, out var definition))
                {
                    definition = new AttentionSignalDefinition
                    {
                        SignalCode = signalCode,
                        SignalCategory = "General",
                        SignalTitle = detail?.SignalLabel ?? signalCode
                    };
                }

                snapshots.Add(new EntityAttentionSignalSnapshot
                {
                    EntityId = entityId,
                    EntityCode = entityCode,
                    SignalCode = signalCode,
                    SignalCategory = definition.SignalCategory,
                    SignalTitle = detail?.SignalLabel ?? definition.SignalTitle ?? signalCode
                });
            }

            return snapshots;
        }

        private static IReadOnlyList<EntityRelationshipSnapshot> BuildRelationshipSnapshots(
            SupplierEntityAnalyticsProduceInput input,
            EntityAnalyticsProduceContext context)
        {
            var portfolio = input?.ManagementAggregate?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
                return Array.Empty<EntityRelationshipSnapshot>();

            var customerLookup = context?.CustomerIdentityLookup;

            var relationshipById = input.RelationshipAggregate?.BySupplierId
                ?? new Dictionary<string, DashboardSupplierRelationshipSupplierRollup>(StringComparer.OrdinalIgnoreCase);

            var snapshots = new List<EntityRelationshipSnapshot>();

            foreach (var supplier in portfolio)
            {
                if (string.IsNullOrWhiteSpace(supplier.SupplierId))
                    continue;

                var entityId = supplier.SupplierId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(supplier.SupplierCode)
                    ? entityId
                    : supplier.SupplierCode.Trim();

                if (!relationshipById.TryGetValue(entityId, out var rollup))
                    continue;

                foreach (var customer in rollup.TopCustomers)
                {
                    var customerIdentity = EntityAnalyticsCustomerIdentityResolver.Resolve(
                        customer.CustomerCode,
                        customerLookup,
                        customerName: customer.CustomerName);

                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopCustomersByOmzet,
                        TargetEntityType = EntityTypeCode.Customer,
                        TargetEntityId = customerIdentity.CustomerId,
                        TargetEntityCode = customerIdentity.CustomerCode,
                        TargetDisplayName = customer.CustomerName ?? customerIdentity.CustomerCode,
                        MetricValue = customer.MetricValue
                    });
                }

                foreach (var salesman in rollup.TopSalesmen)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopSalesmenByOmzet,
                        TargetEntityType = EntityTypeCode.Salesman,
                        TargetEntityId = salesman.SalesPersonId,
                        TargetEntityCode = salesman.SalesPersonCode ?? salesman.SalesPersonId,
                        TargetDisplayName = salesman.SalesPersonName ?? salesman.SalesPersonCode,
                        MetricValue = salesman.MetricValue
                    });
                }

                foreach (var item in rollup.TopItems)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopProductsByOmzet,
                        TargetEntityType = EntityTypeCode.Item,
                        TargetEntityId = item.BrgId,
                        TargetEntityCode = item.BrgCode ?? item.BrgId,
                        TargetDisplayName = item.BrgName,
                        MetricValue = item.MetricValue
                    });
                }
            }

            return snapshots;
        }

        private static IEnumerable<EntityAnalyticsCurrentRow> BuildSupplierRows(
            DashboardPurchasingManagementPortfolioRow supplier,
            string entityId,
            string entityCode,
            DateTime generatedAt,
            IReadOnlyDictionary<string, HashSet<string>> attentionById)
        {
            var rows = new List<EntityAnalyticsCurrentRow>();

            foreach (var (kpiId, value) in BuildTrendKpiValues(supplier))
            {
                rows.Add(CreateRow(entityId, entityCode, kpiId, value, null, generatedAt));
            }

            if (supplier.InventoryValue.HasValue)
            {
                rows.Add(CreateRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.InventoryValue,
                    supplier.InventoryValue,
                    null,
                    generatedAt));
            }

            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, supplier.SupplierName, generatedAt));
            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, supplier.IsActiveMtd ? 1m : 0m, null, generatedAt));

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.PurchaseShare, supplier.PercentOfPurchase, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.AtRiskValue, supplier.AtRiskValue, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveMtd, supplier.IsActiveMtd ? "Yes" : "No", generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveSkuCount, supplier.ActiveSkuCount.ToString(CultureInfo.InvariantCulture), generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.CatalogPenetration, supplier.CatalogPenetrationPercent, generatedAt);

            if (attentionById.TryGetValue(entityId, out var signals) && signals.Count > 0)
            {
                var signalList = string.Join(", ", signals.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
                AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.AttentionSignals, signalList, generatedAt);
            }

            return rows;
        }

        private static void AddDimension(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string dimensionKpiId,
            string value,
            DateTime generatedAt)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            rows.Add(CreateMetaRow(entityId, entityCode, dimensionKpiId, null, value.Trim(), generatedAt));
        }

        private static void AddDimension(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string dimensionKpiId,
            decimal? value,
            DateTime generatedAt)
        {
            if (!value.HasValue)
                return;

            rows.Add(CreateMetaRow(entityId, entityCode, dimensionKpiId, value, null, generatedAt));
        }

        private static EntityAnalyticsCurrentRow CreateRow(
            string entityId,
            string entityCode,
            string kpiId,
            decimal? numericValue,
            string textValue,
            DateTime generatedAt)
        {
            return new EntityAnalyticsCurrentRow
            {
                EntityAnalyticsCurrentId = Ulid.NewUlid().ToString(),
                EntityType = EntityTypeCode.Supplier,
                EntityId = entityId,
                EntityCode = entityCode,
                KpiId = kpiId,
                NumericValue = numericValue,
                TextValue = textValue,
                DefinitionVersion = 1,
                GeneratedAt = generatedAt
            };
        }

        private static EntityAnalyticsCurrentRow CreateMetaRow(
            string entityId,
            string entityCode,
            string kpiId,
            decimal? numericValue,
            string textValue,
            DateTime generatedAt)
        {
            return CreateRow(entityId, entityCode, kpiId, numericValue, textValue, generatedAt);
        }
    }
}

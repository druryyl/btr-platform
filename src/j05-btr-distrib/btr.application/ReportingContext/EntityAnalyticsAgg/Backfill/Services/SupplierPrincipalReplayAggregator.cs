using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services
{
    /// <summary>
    /// Reconstructs Principal KPI aggregates for one replay (closed) month from
    /// month-scoped evidence using the exact live aggregators/composers
    /// (formula fidelity — no replay-specific KPI math).
    /// Scope: PRN-SALES-001, PRN-RET-001..004, PRN-TGT-001..003, PRN-PUR-001.
    /// Non-goals (separate semantic decisions): PRN-TGT-004, PRN-GRW-*, PRN-INV-*, PRN-CUS-*.
    /// </summary>
    public class SupplierPrincipalReplayAggregator
    {
        private readonly PrincipalSalesOutAggregator _salesOutAggregator;
        private readonly PrincipalReturnAggregator _returnAggregator;
        private readonly PrincipalReturnPercentageComposer _returnPercentageComposer;
        private readonly PrincipalTargetAggregator _targetAggregator;
        private readonly PrincipalAchievementComposer _achievementComposer;
        private readonly PrincipalPurchaseInAggregator _purchaseInAggregator;

        public SupplierPrincipalReplayAggregator(
            PrincipalSalesOutAggregator salesOutAggregator,
            PrincipalReturnAggregator returnAggregator,
            PrincipalReturnPercentageComposer returnPercentageComposer,
            PrincipalTargetAggregator targetAggregator,
            PrincipalAchievementComposer achievementComposer,
            PrincipalPurchaseInAggregator purchaseInAggregator)
        {
            _salesOutAggregator = salesOutAggregator;
            _returnAggregator = returnAggregator;
            _returnPercentageComposer = returnPercentageComposer;
            _targetAggregator = targetAggregator;
            _achievementComposer = achievementComposer;
            _purchaseInAggregator = purchaseInAggregator;
        }

        public SupplierPrincipalReplayResult Aggregate(
            SupplierReplayDataBundle bundle,
            Periode periode,
            int year,
            int month,
            DateTime generatedAt)
        {
            if (bundle is null)
                throw new ArgumentNullException(nameof(bundle));
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var salesOut = _salesOutAggregator.Aggregate(
                bundle.SalesOutEvidence, periode, generatedAt);

            var returns = _returnAggregator.Aggregate(
                bundle.ReturnEvidence, year, month, generatedAt);

            var returnPercentage = _returnPercentageComposer.Compose(
                returns, salesOut, generatedAt);

            var targets = _targetAggregator.Aggregate(
                bundle.TargetEvidence, year, month, generatedAt);

            var achievement = _achievementComposer.Compose(
                targets, salesOut, generatedAt);
            achievement = RestrictAchievementToTargetedSuppliers(achievement, targets, generatedAt);

            var purchaseIn = _purchaseInAggregator.Aggregate(
                bundle.PurchaseEvidence, year, month, generatedAt);

            return new SupplierPrincipalReplayResult
            {
                PeriodYear = year,
                PeriodMonth = month,
                SalesOut = salesOut,
                Returns = returns,
                ReturnPercentage = returnPercentage,
                Targets = targets,
                Achievement = achievement,
                PurchaseIn = purchaseIn
            };
        }

        /// <summary>
        /// Decision A: a missing target is semantically different from a zero target.
        /// The live composer emits union(sales, targets) rows with null achievement
        /// values for sales-only suppliers; replay instead emits no TGT-001/002/003
        /// rows at all for suppliers without target evidence, so the UI shows
        /// "no target available" instead of a null achievement calculation.
        /// Formulas are unchanged — emission only is gated.
        /// </summary>
        private static PrincipalAchievementResult RestrictAchievementToTargetedSuppliers(
            PrincipalAchievementResult achievement,
            PrincipalTargetAggregateResult targets,
            DateTime generatedAt)
        {
            if (achievement is null)
                return null;

            var targetedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets?.Principals ?? Enumerable.Empty<PrincipalTargetRow>())
            {
                if (string.IsNullOrWhiteSpace(target?.SupplierId))
                    continue;

                targetedIds.Add(target.SupplierId.Trim());
            }

            var filtered = (achievement.Principals ?? Enumerable.Empty<PrincipalAchievementRow>())
                .Where(row => !string.IsNullOrWhiteSpace(row?.SupplierId)
                    && targetedIds.Contains(row.SupplierId.Trim()))
                .ToList();

            return new PrincipalAchievementResult
            {
                AchievementAmountKpiId = achievement.AchievementAmountKpiId,
                AchievementPercentageKpiId = achievement.AchievementPercentageKpiId,
                SalesOutKpiId = achievement.SalesOutKpiId,
                TargetKpiId = achievement.TargetKpiId,
                PeriodYear = achievement.PeriodYear,
                PeriodMonth = achievement.PeriodMonth,
                GeneratedAt = generatedAt,
                Principals = filtered
            };
        }
    }
}

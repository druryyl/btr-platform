using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.application.SalesContext.SalesPersonSupplierAgg.Contracts;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.nuna.Application;

namespace btr.application.SalesContext.SalesPersonPrincipalTargetAgg
{
    public interface ISalesPersonPrincipalTargetWriter
    {
        void Save(
            string salesPersonId,
            int year,
            int month,
            IEnumerable<SalesPersonPrincipalTargetViewDto> targets);

        int CopyPreviousMonthForSalesPerson(
            string salesPersonId,
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth);

        int CopyPreviousMonthForAllSalesPersons(
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth);
    }

    public class SalesPersonPrincipalTargetWriter : ISalesPersonPrincipalTargetWriter
    {
        private const decimal MaxTargetAmount = 9999999999999999.99m;

        private readonly ISalesPersonPrincipalTargetDal _targetDal;
        private readonly ISalesPersonSupplierDal _salesPersonSupplierDal;
        private readonly ISalesPersonDal _salesPersonDal;

        public SalesPersonPrincipalTargetWriter(
            ISalesPersonPrincipalTargetDal targetDal,
            ISalesPersonSupplierDal salesPersonSupplierDal,
            ISalesPersonDal salesPersonDal)
        {
            _targetDal = targetDal;
            _salesPersonSupplierDal = salesPersonSupplierDal;
            _salesPersonDal = salesPersonDal;
        }

        public void Save(
            string salesPersonId,
            int year,
            int month,
            IEnumerable<SalesPersonPrincipalTargetViewDto> targets)
        {
            ValidatePeriod(year, month);
            ValidateSalesPerson(salesPersonId);

            var assignedSupplierIds = GetAssignedSupplierIds(salesPersonId);
            var rows = (targets ?? Enumerable.Empty<SalesPersonPrincipalTargetViewDto>())
                .Select(x => new SalesPersonPrincipalTargetModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = x.SupplierId,
                    TargetYear = year,
                    TargetMonth = month,
                    TargetAmount = x.TargetAmount
                })
                .ToList();

            foreach (var row in rows)
            {
                ValidateAmount(row.TargetAmount);

                if (!assignedSupplierIds.Contains(row.SupplierId))
                {
                    throw new InvalidOperationException(
                        $"Principal {row.SupplierId} is not assigned to this sales person.");
                }
            }

            using (var trans = TransHelper.NewScope())
            {
                if (rows.Count > 0)
                    _targetDal.Upsert(rows);
                trans.Complete();
            }
        }

        public int CopyPreviousMonthForSalesPerson(
            string salesPersonId,
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth)
        {
            using (var trans = TransHelper.NewScope())
            {
                var copied = CopyPreviousMonthCore(
                    salesPersonId, sourceYear, sourceMonth, targetYear, targetMonth);
                trans.Complete();
                return copied;
            }
        }

        public int CopyPreviousMonthForAllSalesPersons(
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth)
        {
            ValidatePeriod(sourceYear, sourceMonth);
            ValidatePeriod(targetYear, targetMonth);

            var salesPersons = _salesPersonDal.ListData()?.ToList()
                ?? new List<SalesPersonModel>();

            var affectedCount = 0;
            using (var trans = TransHelper.NewScope())
            {
                foreach (var salesPerson in salesPersons)
                {
                    if (string.IsNullOrWhiteSpace(salesPerson.SalesPersonId))
                        continue;

                    var copied = CopyPreviousMonthCore(
                        salesPerson.SalesPersonId,
                        sourceYear,
                        sourceMonth,
                        targetYear,
                        targetMonth);

                    if (copied > 0)
                        affectedCount++;
                }

                trans.Complete();
            }

            return affectedCount;
        }

        private int CopyPreviousMonthCore(
            string salesPersonId,
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth)
        {
            ValidatePeriod(sourceYear, sourceMonth);
            ValidatePeriod(targetYear, targetMonth);
            ValidateSalesPerson(salesPersonId);

            var assignedSupplierIds = GetAssignedSupplierIds(salesPersonId);
            if (assignedSupplierIds.Count == 0)
                return 0;

            var sourceTargets = _targetDal
                .ListBySalesPersonPeriod(salesPersonId, sourceYear, sourceMonth)
                ?.ToDictionary(x => x.SupplierId, x => x.TargetAmount, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            var rows = assignedSupplierIds
                .Select(supplierId => new SalesPersonPrincipalTargetModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = supplierId,
                    TargetYear = targetYear,
                    TargetMonth = targetMonth,
                    TargetAmount = sourceTargets.TryGetValue(supplierId, out var amount) ? amount : 0m
                })
                .ToList();

            _targetDal.Upsert(rows);
            return rows.Count;
        }

        private HashSet<string> GetAssignedSupplierIds(string salesPersonId)
        {
            var ids = _salesPersonSupplierDal
                .ListData(new SalesPersonModel(salesPersonId))
                ?.Select(x => x.SupplierId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
                ?? new List<string>();

            return new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        }

        private void ValidateSalesPerson(string salesPersonId)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
                throw new ArgumentException("Sales person is required.");

            var salesPerson = _salesPersonDal.GetData(new SalesPersonModel(salesPersonId));
            if (salesPerson is null)
                throw new InvalidOperationException("Unknown sales person.");
        }

        private static void ValidatePeriod(int year, int month)
        {
            if (year < 2000 || year > 2100)
                throw new ArgumentOutOfRangeException(nameof(year), "Invalid year.");

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Invalid month.");
        }

        private static void ValidateAmount(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            if (amount > MaxTargetAmount)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount is too large.");
        }
    }
}

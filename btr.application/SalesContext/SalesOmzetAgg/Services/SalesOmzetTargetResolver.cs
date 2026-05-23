using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public class SalesOmzetTargetResolver : ISalesOmzetTargetResolver
    {
        private readonly ISalesOmzetTargetDal _targetDal;
        private readonly ISalesPersonDal _salesPersonDal;

        public SalesOmzetTargetResolver(
            ISalesOmzetTargetDal targetDal,
            ISalesPersonDal salesPersonDal)
        {
            _targetDal = targetDal;
            _salesPersonDal = salesPersonDal;
        }

        public decimal? ResolveTarget(
            IEnumerable<SalesOmzetView> filteredRows,
            string searchKeyword,
            Periode periode,
            string currentUserDisplayName)
        {
            var salesPersonId = ResolveSalesPersonId(filteredRows, searchKeyword, currentUserDisplayName);
            if (string.IsNullOrEmpty(salesPersonId))
                return null;

            var (year, month) = ResolveTargetYearMonth(periode);
            return _targetDal.GetTargetAmount(salesPersonId, year, month);
        }

        public static (int Year, int Month) ResolveTargetYearMonth(Periode periode)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var end = periode.Tgl2.Date;
            return (end.Year, end.Month);
        }

        internal string ResolveSalesPersonId(
            IEnumerable<SalesOmzetView> filteredRows,
            string searchKeyword,
            string currentUserDisplayName)
        {
            var rows = filteredRows?.ToList() ?? new List<SalesOmzetView>();
            var allSalesPersons = _salesPersonDal.ListData()?.ToList() ?? new List<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>();

            var distinctNames = rows
                .Select(r => r.SalesPersonName?.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctNames.Count == 1)
                return FindIdByName(allSalesPersons, distinctNames[0]);

            var keyword = searchKeyword?.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                var keywordLower = keyword.ToLowerInvariant();
                var searchMatches = allSalesPersons
                    .Where(sp => MatchesSalesPersonKeyword(sp.SalesPersonName, keywordLower))
                    .ToList();

                if (searchMatches.Count == 1)
                    return searchMatches[0].SalesPersonId;

                if (searchMatches.Count == 0)
                {
                    var nameMatch = allSalesPersons
                        .Where(sp => string.Equals(sp.SalesPersonName, keyword, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (nameMatch.Count == 1)
                        return nameMatch[0].SalesPersonId;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentUserDisplayName))
            {
                var userMatch = allSalesPersons
                    .Where(sp => string.Equals(
                        sp.SalesPersonName,
                        currentUserDisplayName.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (userMatch.Count == 1)
                    return userMatch[0].SalesPersonId;
            }

            return null;
        }

        private static bool MatchesSalesPersonKeyword(string salesPersonName, string keywordLower)
        {
            if (string.IsNullOrEmpty(salesPersonName))
                return false;

            var nameLower = salesPersonName.ToLowerInvariant();
            if (nameLower.Contains(keywordLower))
                return true;

            return keywordLower
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .All(word => nameLower.Contains(word));
        }

        private static string FindIdByName(
            List<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel> allSalesPersons,
            string salesPersonName)
        {
            var match = allSalesPersons
                .Where(sp => string.Equals(sp.SalesPersonName, salesPersonName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return match.Count == 1 ? match[0].SalesPersonId : null;
        }
    }
}

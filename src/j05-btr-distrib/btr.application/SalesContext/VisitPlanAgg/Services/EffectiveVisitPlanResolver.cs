using System;
using System.Collections.Generic;
using System.Linq;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.SalesContext.VisitPlanAgg.Services
{
    public class EffectiveVisitPlanResolver : IEffectiveVisitPlanResolver
    {
        public IEnumerable<EffectiveVisitPlanEntry> Resolve(
            IEnumerable<VisitPlanModel> basePlan,
            IEnumerable<VisitPlanExceptionModel> exceptions)
        {
            var result = (basePlan ?? Enumerable.Empty<VisitPlanModel>())
                .Select(x => new EffectiveVisitPlanEntry
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    CustomerCode = x.CustomerCode,
                    NoUrut = x.NoUrut,
                    Origin = "Template"
                })
                .ToDictionary(x => x.CustomerId, x => x);

            var exceptionList = (exceptions ?? Enumerable.Empty<VisitPlanExceptionModel>()).ToList();

            foreach (var exception in exceptionList.Where(x =>
                         string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Remove.ToString(),
                             StringComparison.OrdinalIgnoreCase)))
            {
                result.Remove(exception.CustomerId);
            }

            foreach (var exception in exceptionList.Where(x =>
                         string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Replace.ToString(),
                             StringComparison.OrdinalIgnoreCase)))
            {
                result.Remove(exception.CustomerId);
                if (!string.IsNullOrWhiteSpace(exception.ReplacementCustomerId) &&
                    !result.ContainsKey(exception.ReplacementCustomerId))
                {
                    result[exception.ReplacementCustomerId] = new EffectiveVisitPlanEntry
                    {
                        CustomerId = exception.ReplacementCustomerId,
                        CustomerName = exception.ReplacementCustomerName,
                        NoUrut = GetNextNoUrut(result.Values),
                        Origin = "Replaced"
                    };
                }
            }

            foreach (var exception in exceptionList.Where(x =>
                         string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Add.ToString(),
                             StringComparison.OrdinalIgnoreCase)))
            {
                if (!result.ContainsKey(exception.CustomerId))
                {
                    result[exception.CustomerId] = new EffectiveVisitPlanEntry
                    {
                        CustomerId = exception.CustomerId,
                        CustomerName = exception.CustomerName,
                        NoUrut = GetNextNoUrut(result.Values),
                        Origin = "Added"
                    };
                }
            }

            return result.Values
                .OrderBy(x => x.NoUrut)
                .ThenBy(x => x.CustomerId, StringComparer.Ordinal)
                .ToList();
        }

        private static int GetNextNoUrut(IEnumerable<EffectiveVisitPlanEntry> entries)
        {
            var max = entries.Select(x => x.NoUrut).DefaultIfEmpty(0).Max();
            return max + 1;
        }
    }
}

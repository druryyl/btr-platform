using System.Collections.Generic;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;

namespace btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts
{
    public interface ISalesPersonPrincipalTargetDal
    {
        IEnumerable<SalesPersonPrincipalTargetModel> ListBySalesPersonPeriod(
            string salesPersonId, int year, int month);

        IEnumerable<SalesPersonPrincipalTargetModel> ListByPeriod(int year, int month);

        void Upsert(IEnumerable<SalesPersonPrincipalTargetModel> rows);

        decimal SumBySalesPersonPeriod(string salesPersonId, int year, int month);

        IReadOnlyDictionary<string, decimal> SumByPeriod(int year, int month);
    }
}

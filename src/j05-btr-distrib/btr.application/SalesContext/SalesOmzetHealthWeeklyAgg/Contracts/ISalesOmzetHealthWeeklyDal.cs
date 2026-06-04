using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Infrastructure;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts
{
    public interface ISalesOmzetHealthWeeklyDal :
        IInsert<SalesOmzetHealthWeeklyModel>,
        IUpdate<SalesOmzetHealthWeeklyModel>,
        IGetData<SalesOmzetHealthWeeklyModel, ISalesOmzetHealthWeeklyKey>
    {
        SalesOmzetHealthWeeklyModel GetByYearWeek(int yearNumber, int weekNumber);

        IReadOnlyList<SalesOmzetHealthWeeklyModel> ListByYearWeeks(IEnumerable<IsoWeekIdentifier> weeks);
    }
}

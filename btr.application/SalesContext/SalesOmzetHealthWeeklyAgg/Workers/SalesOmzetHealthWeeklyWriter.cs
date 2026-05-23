using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Workers
{
    public interface ISalesOmzetHealthWeeklyWriter : INunaWriter<SalesOmzetHealthWeeklyModel> { }

    public class SalesOmzetHealthWeeklyWriter : ISalesOmzetHealthWeeklyWriter
    {
        private readonly ISalesOmzetHealthWeeklyDal _dal;
        private readonly INunaCounterBL _counter;

        public SalesOmzetHealthWeeklyWriter(
            ISalesOmzetHealthWeeklyDal dal,
            INunaCounterBL counter)
        {
            _dal = dal;
            _counter = counter;
        }

        public void Save(ref SalesOmzetHealthWeeklyModel model)
        {
            if (model.HealthWeeklyId.IsNullOrEmpty())
                model.HealthWeeklyId = _counter.Generate("OHW", IDFormatEnum.PFnnn);

            var persisted = _dal.GetData(model);
            if (persisted is null)
                _dal.Insert(model);
            else
                _dal.Update(model);
        }
    }
}

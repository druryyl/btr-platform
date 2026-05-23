using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Workers
{
    public interface ISalesOmzetWriter : INunaWriter<SalesOmzetModel>
    {
    }

    public class SalesOmzetWriter : ISalesOmzetWriter
    {
        private readonly ISalesOmzetEntityDal _salesOmzetEntityDal;
        private readonly INunaCounterBL _counter;

        public SalesOmzetWriter(
            ISalesOmzetEntityDal salesOmzetEntityDal,
            INunaCounterBL counter)
        {
            _salesOmzetEntityDal = salesOmzetEntityDal;
            _counter = counter;
        }

        public void Save(ref SalesOmzetModel model)
        {
            if (model.SalesOmzetId.IsNullOrEmpty())
                model.SalesOmzetId = _counter.Generate("OMZT", IDFormatEnum.PREFYYMnnnnnC);

            var persisted = _salesOmzetEntityDal.GetData(model);
            if (persisted is null)
                _salesOmzetEntityDal.Insert(model);
            else
                _salesOmzetEntityDal.Update(model);
        }
    }
}

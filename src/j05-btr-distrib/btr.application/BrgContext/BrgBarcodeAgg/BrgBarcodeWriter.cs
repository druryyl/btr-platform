using System;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using FluentValidation;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public interface IBrgBarcodeWriter : INunaWriter<BrgBarcodeModel>
    {
    }

    public class BrgBarcodeWriter : IBrgBarcodeWriter
    {
        private readonly IValidator<BrgBarcodeModel> _validator;
        private readonly IBrgBarcodeDal _brgBarcodeDal;

        public BrgBarcodeWriter(IValidator<BrgBarcodeModel> validator,
            IBrgBarcodeDal brgBarcodeDal)
        {
            _validator = validator;
            _brgBarcodeDal = brgBarcodeDal;
        }

        public void Save(ref BrgBarcodeModel model)
        {
            _validator.ValidateAndThrow(model);

            if (model.BrgBarcodeId.IsNullOrEmpty())
                model.BrgBarcodeId = Ulid.NewUlid().ToString();

            using (var trans = TransHelper.NewScope())
            {
                var db = _brgBarcodeDal.GetData(model);
                if (db is null)
                    _brgBarcodeDal.Insert(model);
                else
                    _brgBarcodeDal.Update(model);

                trans.Complete();
            }
        }
    }
}

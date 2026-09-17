using System.Linq;
using btr.application.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using FluentValidation;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public class BrgBarcodeValidator : AbstractValidator<BrgBarcodeModel>
    {
        public BrgBarcodeValidator(IBrgDal brgDal,
            IBrgSatuanDal brgSatuanDal)
        {
            RuleFor(x => x.BarcodeValue)
                .Must(x => !string.IsNullOrWhiteSpace(BrgBarcodeModel.NormalizeBarcodeValue(x)))
                .WithMessage("BarcodeValue is required");

            RuleFor(x => x.BrgId)
                .Must(x => !string.IsNullOrWhiteSpace(x)
                           && brgDal.GetData(new BrgModel(x)) != null)
                .WithMessage("BrgId not found");

            RuleFor(x => x)
                .Must(x => SatuanExists(brgSatuanDal, x))
                .When(x => !string.IsNullOrWhiteSpace(x.Satuan))
                .WithMessage("Satuan not found for Brg");
        }

        private static bool SatuanExists(IBrgSatuanDal brgSatuanDal, BrgBarcodeModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BrgId))
                return false;
            var listSatuan = brgSatuanDal.ListData((IBrgKey)new BrgModel(model.BrgId));
            return listSatuan != null
                   && listSatuan.Any(x => x.Satuan == model.Satuan);
        }
    }
}

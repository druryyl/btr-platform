using System.Linq;
using btr.application.BrgContext.BrgAgg;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.SalesContext.CustomerAgg;
using FluentValidation;

namespace btr.application.InventoryContext.ReturnOrderAgg.Workers
{
    public class ReturnOrderValidator : AbstractValidator<ReturnOrderModel>
    {
        public ReturnOrderValidator(ICustomerDal customerDal,
            IBrgDal brgDal,
            IBrgSatuanDal brgSatuanDal)
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer wajib diisi")
                .Must(x => !string.IsNullOrWhiteSpace(x)
                           && customerDal.GetData(new CustomerModel(x)) != null)
                .WithMessage("Customer tidak ditemukan");

            RuleFor(x => x.WarehouseCode)
                .NotEmpty().WithMessage("Warehouse wajib diisi");

            RuleFor(x => x.ListItem)
                .Must(x => x != null && x.Any())
                .WithMessage("Return Order harus memiliki minimal 1 item");

            RuleForEach(x => x.ListItem).ChildRules(item =>
            {
                item.RuleFor(x => x.BrgId)
                    .NotEmpty().WithMessage("Barang wajib diisi")
                    .Must(x => !string.IsNullOrWhiteSpace(x)
                               && brgDal.GetData(new BrgModel(x)) != null)
                    .WithMessage("Barang tidak ditemukan");

                item.RuleFor(x => x.Qty)
                    .GreaterThan(0).WithMessage("Qty harus lebih besar dari 0");

                item.RuleFor(x => x.SatId)
                    .NotEmpty().WithMessage("Satuan wajib diisi");

                item.RuleFor(x => x)
                    .Must(x => SatuanExists(brgSatuanDal, x))
                    .WithMessage("Satuan tidak valid untuk barang");

                item.RuleFor(x => x.JenisRetur)
                    .Must(x => x == "BAGUS" || x == "RUSAK")
                    .WithMessage("Jenis Retur harus BAGUS atau RUSAK");
            });
        }

        private static bool SatuanExists(IBrgSatuanDal brgSatuanDal, ReturnOrderItemModel item)
        {
            if (string.IsNullOrWhiteSpace(item.BrgId))
                return false;
            var listSatuan = brgSatuanDal.ListData((IBrgKey)new BrgModel(item.BrgId));
            return listSatuan != null
                   && listSatuan.Any(x => x.Satuan == item.SatId);
        }
    }
}

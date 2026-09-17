using btr.application.BrgContext.BrgAgg;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.application.InventoryContext.DriverAgg;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.infrastructure.BrgContext.BrgAgg;
using btr.infrastructure.BrgContext.BrgBarcodeAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.InventoryContext.DriverAgg;
using btr.infrastructure.InventoryContext.ReturnOrderAgg;
using btr.infrastructure.InventoryContext.WarehouseAgg;
using btr.infrastructure.SalesContext.CustomerAgg;
using btr.infrastructure.SalesContext.SalesPersonAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OfficeReturnOrderItemModel = btr.domain.InventoryContext.ReturnOrderAgg.ReturnOrderItemModel;
using OfficeReturnOrderModel = btr.domain.InventoryContext.ReturnOrderAgg.ReturnOrderModel;

namespace j07_btrade_sync.Shared
{
    //  IR-08 — the synchronization client references btr.application /
    //  btr.infrastructure directly and invokes the Main Office application
    //  command in-process, so authoritative validation (BR-011, P-01) exists
    //  in exactly one place.
    //  S3.4 (Arch §4.3, §8.2, I-RO-08, IR-RO-05) — the Return Order import
    //  dispatch follows the same IR-08 pattern: numbering (ReturnOrderNo,
    //  IR-RO-07/ADR-RO-002) and WarehouseCode persistence live in the office
    //  ImportReturnOrderCommand only; the sync client performs no numbering
    //  or mapping (ADR-RO-002/008).
    public class MainOfficeCommandExecutor
    {
        private readonly IMediator _mediator;

        public MainOfficeCommandExecutor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<DatabaseOptions>>(
                Options.Create(new DatabaseOptions()));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
                typeof(ProcessBarcodeRegistrationRequestHandler).Assembly));
            services.AddScoped<DateTimeProvider>();
            services.AddScoped<IValidator<BrgBarcodeModel>, BrgBarcodeValidator>();
            services.AddScoped<IBrgDal, BrgDal>();
            services.AddScoped<IBrgSatuanDal, BrgSatuanDal>();
            services.AddScoped<IBrgBarcodeDal, BrgBarcodeDal>();
            services.AddScoped<IBrgBarcodeBuilder, BrgBarcodeBuilder>();
            services.AddScoped<IBrgBarcodeWriter, BrgBarcodeWriter>();

            //  S3.4 — Return Order import components (S1.5 command closure):
            //  DALs, builder/writer/validator, counter (+ master DALs required
            //  by the validator/builder). MediatR handlers are already covered
            //  by the assembly scan above (same btr.application assembly).
            services.AddScoped<IReturnOrderDal, ReturnOrderDal>();
            services.AddScoped<IReturnOrderItemDal, ReturnOrderItemDal>();
            services.AddScoped<IReturnOrderWriter, ReturnOrderWriter>();
            services.AddScoped<IReturnOrderBuilder, ReturnOrderBuilder>();
            services.AddScoped<IValidator<OfficeReturnOrderModel>, ReturnOrderValidator>();
            services.AddScoped<INunaCounterBL, NunaCounterBL>();
            services.AddScoped<INunaCounterDal, ParamNoDal>();
            services.AddScoped<ICustomerDal, CustomerDal>();
            services.AddScoped<IWarehouseDal, WarehouseDal>();
            services.AddScoped<ISalesPersonDal, SalesPersonDal>();
            services.AddScoped<IDriverDal, DriverDal>();

            var provider = services.BuildServiceProvider();
            _mediator = provider.GetRequiredService<IMediator>();
        }

        public Task<ProcessBarcodeRegistrationRequestResponse> ProcessBarcodeRegistration(
            string barcodeValue, string brgId, string satuan, string userId)
        {
            return _mediator.Send(new ProcessBarcodeRegistrationRequestCommand(
                barcodeValue, brgId, satuan, userId));
        }

        //  S3.4 — in-process import dispatch (I-RO-08). The downloaded transport
        //  model is mapped onto ImportReturnOrderCommand; numbering and
        //  WarehouseCode persistence happen office-side only.
        public Task<ImportReturnOrderResponse> ImportReturnOrder(
            Model.ReturnOrderModel order, string userId)
        {
            var listItem = (order.ListItems ?? Enumerable.Empty<Model.ReturnOrderItemType>())
                .Select(x => new OfficeReturnOrderItemModel
                {
                    BrgId = x.BrgId ?? string.Empty,
                    BrgCode = x.BrgCode ?? string.Empty,
                    BrgName = x.BrgName ?? string.Empty,
                    Qty = x.Qty,
                    SatId = x.SatId ?? string.Empty,
                    JenisRetur = x.JenisRetur ?? string.Empty
                })
                .ToList();

            return _mediator.Send(new ImportReturnOrderCommand(
                order.ReturnOrderId,
                ToDate(order.ReturnOrderDate),
                order.WarehouseCode ?? string.Empty,
                order.CustomerId ?? string.Empty,
                order.SalesPersonId ?? string.Empty,
                order.DriverId ?? string.Empty,
                order.Note ?? string.Empty,
                listItem,
                userId ?? string.Empty));
        }

        private static DateTime ToDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;
            if (DateTime.TryParse(value, out result))
                return result;
            return new DateTime(3000, 1, 1);
        }
    }
}

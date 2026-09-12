using btr.application.SalesContext.FakturAgg.Workers;
using btr.application.SalesContext.OrderFeature;
using btr.domain.SalesContext.FakturAgg;
using btr.domain.SalesContext.OrderAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.FakturAgg.UseCases
{
    /// <summary>
    /// Single shared in-memory aggregate construction path (SL-03, D-001/D-006/D-007/D-011).
    /// Form Input → SaveFakturRequest → FakturBuilder → FakturModel.
    /// Used identically by preview and save; preview is a side-effect-free view
    /// of the same aggregate that save will persist.
    /// Performs reads + in-memory CalcTotal only. Never calls FakturWriter.Save,
    /// IGenStokFakturWorker, piutang/packing/order updates, commits, or counters.
    /// </summary>
    public interface IBuildFakturAggregateWorker : INunaService<FakturModel, SaveFakturRequest> { }

    public class BuildFakturAggregateWorker : IBuildFakturAggregateWorker
    {
        private readonly IFakturBuilder _fakturBuilder;

        public BuildFakturAggregateWorker(IFakturBuilder fakturBuilder)
        {
            _fakturBuilder = fakturBuilder;
        }

        public FakturModel Execute(SaveFakturRequest req)
        {
            //  BUILD (shared verbatim construction; save delegates here, preview calls here)
            FakturModel result;
            if (req.FakturId.Length == 0)
            {
                //  NEW: number deferred to save (empty FakturId); pre-selected open code carried via req.FakturCode below
                result = _fakturBuilder.CreateNew(req).Build();
            }
            else
            {
                //  EDIT: built from current form state (req), carrying existing FakturId/FakturCode;
                //  never the reloaded persisted items (cleared and rebuilt from req below)
                result = _fakturBuilder.Load(req).Build();
                result.ListItem.Clear();
                result.ListItemKlaim.Clear();
            }

            result = _fakturBuilder
                .Attach(result)
                .FakturCode(req.FakturCode)
                .FakturDate(req.FakturDate.ToDate(DateFormatEnum.YMD))
                .Customer(req)
                .Order(OrderModel.Key(req.OrderId))
                .SalesPerson(req)
                .Warehouse(req)
                .TglRencanaKirim(req.RencanaKirimDate.ToDate(DateFormatEnum.YMD))
                .Driver(req)
                .User(req)
                .TermOfPayment((TermOfPaymentEnum)req.TermOfPayment)
                .DueDate(req.DueDate.ToDate(DateFormatEnum.YMD))
                .Cash(req.Cash)
                .Note(req.Note)
                .Build();

            foreach (var item in req.ListBrg)
            {
                result = _fakturBuilder
                    .Attach(result)
                    .AddItem(item, item.StokHarga, item.QtyString, item.HrgString, item.DiscountString, item.DppProsen, item.PpnProsen, false)
                    .Build();
            }

            foreach (var item in req.ListBrgKlaim)
            {
                result = _fakturBuilder
                    .Attach(result)
                    .AddItemKlaim(item, item.StokHarga, item.QtyString, item.HrgString, item.DiscountString, item.DppProsen, item.PpnProsen, false)
                    .Build();
            }


            result = _fakturBuilder
                .Attach(result)
                .CalcTotal()
                .Build();

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.UseCases
{
    public interface IReconcileSalesOmzetWorker : INunaServiceVoid<ReconcileSalesOmzetRequest> { }

    /// <summary>
    /// Sole bulk writer for <c>BTR_SalesOmzet</c>. <see cref="ReconcileSalesOmzetScope.PeriodeScoped"/> limits
    /// source lists to the request periode; <see cref="ReconcileSalesOmzetScope.Full"/> loads all orders/fakturs
    /// (see <see cref="ISalesOmzetSourceDal.ListAllOrders"/>). Large DBs may need batching — see deploy runbook.
    /// </summary>
    public class ReconcileSalesOmzetWorker : IReconcileSalesOmzetWorker
    {
        private readonly ISalesOmzetSourceDal _source;
        private readonly ISalesOmzetLinker _linker;
        private readonly ISalesOmzetEntityDal _entityDal;

        public ReconcileSalesOmzetWorker(
            ISalesOmzetSourceDal source,
            ISalesOmzetLinker linker,
            ISalesOmzetEntityDal entityDal)
        {
            _source = source;
            _linker = linker;
            _entityDal = entityDal;
        }

        public void Execute(ReconcileSalesOmzetRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            if (request.Periode is null)
                throw new ArgumentException("Periode is required.", nameof(request));

            var isFull = request.Scope == ReconcileSalesOmzetScope.Full;
            var sw = Stopwatch.StartNew();
            var ordersProcessed = 0;
            var faktursProcessed = 0;

            var orderList = (isFull
                ? _source.ListAllOrders()
                : _source.ListOrders(request.Periode)).ToList();
            var fakturList = (isFull
                ? _source.ListAllFakturs()
                : _source.ListFakturs(request.Periode)).ToList();

            var touched = new Dictionary<string, SalesOmzetModel>(StringComparer.Ordinal);

            using (var trans = TransHelper.NewScope())
            {
                Report(request, 0, orderList.Count, "Memproses order...");

                foreach (var order in orderList)
                {
                    ordersProcessed++;
                    var row = _linker.FindOrCreateForOrder(order);
                    if (row != null)
                        Touch(touched, row);

                    Report(request, ordersProcessed, orderList.Count, "Memproses order...");
                }

                Report(request, 0, fakturList.Count, "Memproses faktur...");

                foreach (var faktur in fakturList)
                {
                    faktursProcessed++;
                    var row = _linker.FindOrCreateForFaktur(faktur);
                    if (row != null)
                        Touch(touched, row);

                    Report(request, faktursProcessed, fakturList.Count, "Memproses faktur...");
                }

                Report(request, 0, 1, "Menggabungkan data agregat...");

                if (isFull)
                {
                    foreach (var existing in _entityDal.ListAll() ?? Enumerable.Empty<SalesOmzetModel>())
                        MergeExisting(touched, existing);
                }
                else
                {
                    foreach (var existing in _entityDal.ListForReconcileScope(request.Periode) ?? Enumerable.Empty<SalesOmzetModel>())
                        MergeExisting(touched, existing);
                }

                var refreshList = touched.Values.ToList();
                var refreshIndex = 0;

                Report(request, 0, refreshList.Count, "Refresh baris omzet...");

                foreach (var row in refreshList)
                {
                    _linker.Refresh(row);
                    refreshIndex++;
                    Report(request, refreshIndex, refreshList.Count, "Refresh baris omzet...");
                }

                trans.Complete();
            }

            sw.Stop();

            request.Result = new ReconcileSalesOmzetResult
            {
                OrdersProcessed = ordersProcessed,
                FaktursProcessed = faktursProcessed,
                RowsRefreshed = touched.Count,
                RowsCreated = 0,
                Duration = sw.Elapsed,
                Scope = request.Scope
            };

            Trace.WriteLine(
                $"SalesOmzet reconcile ({request.Scope}): orders={ordersProcessed}, fakturs={faktursProcessed}, " +
                $"refreshed={touched.Count}, duration={sw.Elapsed}");
        }

        private static void Report(
            ReconcileSalesOmzetRequest request,
            int current,
            int total,
            string phase)
        {
            request.Progress?.Report(new ReconcileSalesOmzetProgress
            {
                Current = current,
                Total = total,
                Phase = phase
            });
        }

        private static void Touch(Dictionary<string, SalesOmzetModel> touched, SalesOmzetModel row)
        {
            if (!string.IsNullOrEmpty(row.SalesOmzetId))
                touched[row.SalesOmzetId] = row;
        }

        private static void MergeExisting(Dictionary<string, SalesOmzetModel> touched, SalesOmzetModel existing)
        {
            if (existing != null && !string.IsNullOrEmpty(existing.SalesOmzetId))
                touched[existing.SalesOmzetId] = existing;
        }
    }
}

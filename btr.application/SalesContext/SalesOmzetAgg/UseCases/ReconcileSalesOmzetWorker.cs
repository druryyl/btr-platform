using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.UseCases
{
    public interface IReconcileSalesOmzetWorker : INunaServiceVoid<ReconcileSalesOmzetRequest> { }

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

            if (request.Scope == ReconcileSalesOmzetScope.Full)
                throw new NotSupportedException("Full reconcile scope is not implemented yet.");

            var touched = new Dictionary<string, SalesOmzetModel>(StringComparer.Ordinal);

            using (var trans = TransHelper.NewScope())
            {
                foreach (var order in _source.ListOrders(request.Periode))
                {
                    var row = _linker.FindOrCreateForOrder(order);
                    if (row != null)
                        touched[row.SalesOmzetId] = row;
                }

                foreach (var faktur in _source.ListFakturs(request.Periode))
                {
                    var row = _linker.FindOrCreateForFaktur(faktur);
                    if (row != null)
                        touched[row.SalesOmzetId] = row;
                }

                foreach (var existing in _entityDal.ListForReconcileScope(request.Periode))
                {
                    if (existing != null && !string.IsNullOrEmpty(existing.SalesOmzetId))
                        touched[existing.SalesOmzetId] = existing;
                }

                foreach (var row in touched.Values.ToList())
                    _linker.Refresh(row);

                trans.Complete();
            }
        }
    }
}

using System;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.application.SalesContext.SalesOmzetAgg.Workers;
using btr.domain.SalesContext.SalesOmzetAgg;
namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public class SalesOmzetLinker : ISalesOmzetLinker
    {
        private readonly ISalesOmzetEntityDal _entityDal;
        private readonly ISalesOmzetWriter _writer;
        private readonly ISalesOmzetSourceDal _source;
        private readonly ISalesOmzetSnapshotBuilder _snapshotBuilder;
        private readonly ISalesOmzetSaleKindPolicy _saleKindPolicy;
        private readonly ISalesOmzetStatusPolicy _statusPolicy;
        private readonly ISalesOmzetEligibilityPolicy _eligibilityPolicy;

        public SalesOmzetLinker(
            ISalesOmzetEntityDal entityDal,
            ISalesOmzetWriter writer,
            ISalesOmzetSourceDal source,
            ISalesOmzetSnapshotBuilder snapshotBuilder,
            ISalesOmzetSaleKindPolicy saleKindPolicy,
            ISalesOmzetStatusPolicy statusPolicy,
            ISalesOmzetEligibilityPolicy eligibilityPolicy)
        {
            _entityDal = entityDal;
            _writer = writer;
            _source = source;
            _snapshotBuilder = snapshotBuilder;
            _saleKindPolicy = saleKindPolicy;
            _statusPolicy = statusPolicy;
            _eligibilityPolicy = eligibilityPolicy;
        }

        public SalesOmzetModel FindOrCreateForOrder(OrderSnapshot order)
        {
            if (!_eligibilityPolicy.IsOrderEligible(order))
                return null;

            var existing = _entityDal.GetByOrderId(order.OrderId);
            if (existing != null)
                return existing;

            var now = DateTime.Now;
            var row = new SalesOmzetModel
            {
                OrderId = order.OrderId,
                FakturId = string.Empty,
                FakturCode = string.Empty,
                SaleKind = SaleKindEnum.OrderedSale,
                OmzetDate = SalesOmzetDates.Sentinel,
                FakturDate = SalesOmzetDates.Sentinel,
                CustomerName = order.CustomerName,
                Code = string.Empty,
                Alamat = string.Empty,
                CreatedAt = now,
                LastReconciledAt = now
            };

            _snapshotBuilder.SetSalesDateOnCreate(row, SaleKindEnum.OrderedSale, order, null);
            _snapshotBuilder.ApplyOrder(row, order);

            if (!string.IsNullOrEmpty(order.CustomerName))
                row.CustomerName = order.CustomerName;

            row.OmzetStatus = _statusPolicy.Resolve(row);
            _writer.Save(ref row);
            return row;
        }

        public SalesOmzetModel FindOrCreateForFaktur(FakturSnapshot faktur)
        {
            if (!_eligibilityPolicy.IsFakturEligible(faktur))
                return null;

            if (!string.IsNullOrEmpty(faktur.OrderId))
            {
                var row = _entityDal.GetByOrderId(faktur.OrderId);
                if (row == null)
                {
                    var order = _source.GetOrderByOrderId(faktur.OrderId);
                    if (order != null)
                        row = FindOrCreateForOrder(order);
                }

                if (row == null)
                    return null;

                row.FakturId = faktur.FakturId;
                row.SaleKind = SaleKindEnum.OrderedSale;
                _writer.Save(ref row);
                return row;
            }

            var byFaktur = _entityDal.GetByFakturId(faktur.FakturId);
            if (byFaktur != null)
                return byFaktur;

            var now = DateTime.Now;
            var direct = new SalesOmzetModel
            {
                OrderId = string.Empty,
                FakturId = faktur.FakturId,
                SaleKind = SaleKindEnum.DirectSale,
                OmzetDate = SalesOmzetDates.Sentinel,
                OrderDate = SalesOmzetDates.Sentinel,
                CustomerName = string.Empty,
                Code = string.Empty,
                Alamat = string.Empty,
                CreatedAt = now,
                LastReconciledAt = now
            };

            _snapshotBuilder.SetSalesDateOnCreate(direct, SaleKindEnum.DirectSale, null, faktur);

            var customer = _source.GetCustomer(faktur.CustomerId);
            _snapshotBuilder.ApplyFaktur(direct, faktur, customer);
            var statuses = _source.ListControlStatus(faktur.FakturId);
            _snapshotBuilder.ApplyOmzetDate(direct, statuses);

            direct.SaleKind = _saleKindPolicy.Resolve(direct);
            direct.OmzetStatus = _statusPolicy.Resolve(direct);
            _writer.Save(ref direct);
            return direct;
        }

        public void Refresh(SalesOmzetModel row)
        {
            if (row is null) return;

            OrderSnapshot order = null;
            FakturSnapshot faktur = null;

            if (!string.IsNullOrEmpty(row.OrderId))
                order = _source.GetOrderByOrderId(row.OrderId);

            if (!string.IsNullOrEmpty(row.FakturId))
                faktur = _source.GetFakturByFakturId(row.FakturId);
            else if (!string.IsNullOrEmpty(row.OrderId))
                faktur = _source.GetFakturByOrderId(row.OrderId);

            var salesDateBefore = row.SalesDate;

            if (order != null)
                _snapshotBuilder.ApplyOrder(row, order);

            if (faktur != null)
            {
                var customer = _source.GetCustomer(faktur.CustomerId);
                _snapshotBuilder.ApplyFaktur(row, faktur, customer);
                var statuses = _source.ListControlStatus(faktur.FakturId);
                _snapshotBuilder.ApplyOmzetDate(row, statuses);

                if (_eligibilityPolicy.ShouldRemove(row, faktur))
                {
                    row.OmzetStatus = SalesOmzetStatusEnum.Void;
                    row.SalesDate = salesDateBefore;
                    row.LastReconciledAt = DateTime.Now;
                    _writer.Save(ref row);
                    return;
                }
            }

            row.SalesDate = salesDateBefore;
            row.SaleKind = _saleKindPolicy.Resolve(row);
            row.OmzetStatus = _statusPolicy.Resolve(row);
            row.LastReconciledAt = DateTime.Now;
            _writer.Save(ref row);
        }
    }
}

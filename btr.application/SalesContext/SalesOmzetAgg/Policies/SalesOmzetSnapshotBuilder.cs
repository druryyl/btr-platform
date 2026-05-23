using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.FakturControlAgg;
using btr.domain.SalesContext.SalesOmzetAgg;
namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetSnapshotBuilder : ISalesOmzetSnapshotBuilder
    {
        public void ApplyOrder(SalesOmzetModel row, OrderSnapshot order)
        {
            if (row is null || order is null) return;

            row.OrderDate = order.OrderDate;
            row.OrderTotal = order.OrderTotal;
            if (!string.IsNullOrEmpty(order.SalesName))
                row.SalesPersonName = order.SalesName;
            else if (!string.IsNullOrEmpty(order.UserEmail))
                row.SalesPersonName = order.UserEmail;
        }

        public void ApplyFaktur(SalesOmzetModel row, FakturSnapshot faktur, CustomerSnapshot customer)
        {
            if (row is null || faktur is null) return;

            row.FakturCode = faktur.FakturCode ?? string.Empty;
            row.FakturDate = faktur.FakturDate;
            row.FakturTotal = faktur.FakturTotal;

            if (!string.IsNullOrEmpty(faktur.SalesPersonName))
                row.SalesPersonName = faktur.SalesPersonName;

            if (customer != null)
            {
                row.CustomerName = customer.CustomerName ?? string.Empty;
                row.Code = customer.CustomerCode ?? string.Empty;
                row.Alamat = customer.Address1 ?? string.Empty;
            }
        }

        public void ApplyOmzetDate(SalesOmzetModel row, IEnumerable<FakturControlStatusSnapshot> controlStatuses)
        {
            if (row is null) return;

            var kembali = controlStatuses?
                .Where(s => s.StatusFaktur == StatusFakturEnum.KembaliFaktur)
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefault();

            row.OmzetDate = kembali != null ? kembali.StatusDate : SalesOmzetDates.Sentinel;
        }

        public void SetSalesDateOnCreate(SalesOmzetModel row, SaleKindEnum saleKind, OrderSnapshot order, FakturSnapshot faktur)
        {
            if (row is null) return;

            switch (saleKind)
            {
                case SaleKindEnum.OrderedSale:
                    row.SalesDate = order?.OrderDate ?? SalesOmzetDates.Sentinel;
                    break;

                case SaleKindEnum.DirectSale:
                    // Default: Tanggal Jual = FakturDate for direct sales (frozen at create).
                    row.SalesDate = faktur?.FakturDate ?? SalesOmzetDates.Sentinel;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(saleKind), saleKind, null);
            }
        }
    }
}

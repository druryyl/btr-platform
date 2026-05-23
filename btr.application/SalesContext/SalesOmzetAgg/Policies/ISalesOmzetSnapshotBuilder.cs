using System.Collections.Generic;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetSnapshotBuilder
    {
        void ApplyOrder(SalesOmzetModel row, OrderSnapshot order);
        void ApplyFaktur(SalesOmzetModel row, FakturSnapshot faktur, CustomerSnapshot customer);
        void ApplyOmzetDate(SalesOmzetModel row, IEnumerable<FakturControlStatusSnapshot> controlStatuses);

        /// <summary>Called only from linker on insert — never on refresh.</summary>
        void SetSalesDateOnCreate(SalesOmzetModel row, SaleKindEnum saleKind, OrderSnapshot order, FakturSnapshot faktur);
    }
}

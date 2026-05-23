using System.Collections.Generic;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Contracts
{
    /// <summary>
    /// Loads lightweight snapshots for reconcile/linker. Faktur void filter is applied in SQL
    /// (VoidDate = sentinel). Period scoping on list methods is for reconcile scope only — not
    /// used when linking faktur by OrderId.
    /// </summary>
    public interface ISalesOmzetSourceDal
    {
        IEnumerable<OrderSnapshot> ListOrders(Periode periode);
        IEnumerable<FakturSnapshot> ListFakturs(Periode periode);

        /// <summary>All orders — no date filter (full reconcile / backfill).</summary>
        IEnumerable<OrderSnapshot> ListAllOrders();

        /// <summary>All non-void fakturs — no faktur-date filter (full reconcile / backfill).</summary>
        IEnumerable<FakturSnapshot> ListAllFakturs();

        OrderSnapshot GetOrderByOrderId(string orderId);
        FakturSnapshot GetFakturByFakturId(string fakturId);

        /// <summary>First non-void faktur for order; no faktur-date / report-period filter.</summary>
        FakturSnapshot GetFakturByOrderId(string orderId);

        CustomerSnapshot GetCustomer(string customerId);
        IEnumerable<FakturControlStatusSnapshot> ListControlStatus(string fakturId);
    }
}

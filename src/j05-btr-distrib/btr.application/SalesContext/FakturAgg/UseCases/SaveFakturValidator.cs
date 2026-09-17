using System;
using System.Linq;
using btr.nuna.Application;
using Dawn;

namespace btr.application.SalesContext.FakturAgg.UseCases
{
    /// <summary>
    /// Shared save-level validation entry for preview and save (SL-02, D-010).
    /// Covers all SaveFakturWorker business guards plus mandatory form requirements
    /// (customer, warehouse, salesperson, due date, at least one item line).
    /// Reads only; triggers no persistence, commit, or counter consumption (D-007).
    /// </summary>
    public static class SaveFakturValidator
    {
        public static void Validate(SaveFakturRequest req)
        {
            //  GUARD (same rules/messages as Save previously enforced)
            Guard.Argument(() => req).NotNull()
                .Member(x => x.FakturDate, y => y.ValidDate("yyyy-MM-dd"))
                .Member(x => x.CustomerId, y => y.NotEmpty())
                .Member(x => x.SalesPersonId, y => y.NotEmpty())
                .Member(x => x.WarehouseId, y => y.NotEmpty())
                .Member(x => x.DueDate, y => y.ValidDate("yyyy-MM-dd"));

            //  At least one item line across jual + klaim (D-010 validation scope)
            var hasJual = req.ListBrg != null && req.ListBrg.Any();
            var hasKlaim = req.ListBrgKlaim != null && req.ListBrgKlaim.Any();
            if (!hasJual && !hasKlaim)
                throw new ArgumentException(
                    "Faktur must contain at least one item line.", nameof(req.ListBrg));
        }
    }
}

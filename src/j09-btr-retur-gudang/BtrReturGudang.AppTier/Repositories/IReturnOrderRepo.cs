using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.AppTier.Repositories
{
    /// <summary>
    /// Repository contract for ReturnOrder persistence in local SQL database (btrgd).
    /// Enforces transaction boundary when writing header and detail lines.
    /// Traceable to: ARCHITECTURE §5.1, P1-S03, P2-S04.
    /// </summary>
    public interface IReturnOrderRepo
    {
        ReturnOrder GetById(string returnOrderId);
        Task<ReturnOrder> GetByIdAsync(string returnOrderId);

        IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null);
        Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null);

        IEnumerable<ReturnOrder> GetPendingUploadList();
        Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync();

        void Save(ReturnOrder returnOrder);
        Task SaveAsync(ReturnOrder returnOrder);

        void MarkSynced(string returnOrderId, DateTime syncedDate);
        Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate);

        void DeleteDraft(string returnOrderId);
        Task DeleteDraftAsync(string returnOrderId);
    }
}

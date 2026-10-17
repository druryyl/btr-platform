using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.AppTier.Services
{
    /// <summary>
    /// Orchestration service contract for warehouse Return Order workflows.
    /// Handles intake lifecycle: document creation, line addition, line modification,
    /// local draft commitment, and atomic commit + cloud upload.
    /// Traceable to: ARCHITECTURE §5.1, §6, P1-S03, P3-S08.
    /// </summary>
    public interface IReturnOrderService
    {
        ReturnOrderDto Create(
            string custId,
            string custName,
            string warehouseCode,
            string operatorName,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            string customerCode = null,
            string wilayah = null,
            IEnumerable<ReturnOrderItemDto> items = null);

        Task<ReturnOrderDto> CreateAsync(
            string custId,
            string custName,
            string warehouseCode,
            string operatorName,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            string customerCode = null,
            string wilayah = null,
            IEnumerable<ReturnOrderItemDto> items = null);

        ReturnOrderDto Update(
            string returnOrderId,
            string custId,
            string custName,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            string customerCode = null,
            string wilayah = null,
            IEnumerable<ReturnOrderItemDto> items = null);

        Task<ReturnOrderDto> UpdateAsync(
            string returnOrderId,
            string custId,
            string custName,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            string customerCode = null,
            string wilayah = null,
            IEnumerable<ReturnOrderItemDto> items = null);

        ReturnOrderDto AddItem(
            string returnOrderId,
            string brgId,
            string brgName,
            string barcode,
            JenisRetur jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null);

        ReturnOrderDto UpdateLine(
            string returnOrderId,
            int noUrut,
            decimal newQty,
            JenisRetur newJenisRetur,
            string newKeterangan = null);

        ReturnOrderDto UpdateLine(
            string returnOrderId,
            int noUrut,
            decimal newQty,
            string newJenisRetur,
            string newKeterangan = null);

        ReturnOrderDto RemoveLine(
            string returnOrderId,
            int noUrut);

        ReturnOrderDto CommitDraft(string returnOrderId);
        Task<ReturnOrderDto> CommitDraftAsync(string returnOrderId);

        Task<SyncResultDto> CommitAndUploadAsync(string returnOrderId);

        ReturnOrderDto GetById(string returnOrderId);
        Task<ReturnOrderDto> GetByIdAsync(string returnOrderId);

        IEnumerable<ReturnOrderDto> GetOrders(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null);
        Task<IEnumerable<ReturnOrderDto>> GetOrdersAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null);

        void DeleteDraft(string returnOrderId);
        Task DeleteDraftAsync(string returnOrderId);
    }
}

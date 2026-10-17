using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Common;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;

namespace BtrReturGudang.AppTier.Services
{
    /// <summary>
    /// Application orchestration service implementing Return Order business workflows.
    /// Interacts directly with domain entities to uphold domain invariants, persistence via IReturnOrderRepo,
    /// and triggers synchronization via ISyncService.
    /// Traceable to: ARCHITECTURE §5.1, §6, P1-S03.
    /// </summary>
    public class ReturnOrderService : IReturnOrderService
    {
        private readonly IReturnOrderRepo _returnOrderRepo;
        private readonly ISyncService _syncService;

        public ReturnOrderService(
            IReturnOrderRepo returnOrderRepo,
            ISyncService syncService = null)
        {
            _returnOrderRepo = returnOrderRepo ?? throw new ArgumentNullException(nameof(returnOrderRepo));
            _syncService = syncService;
        }

        public ReturnOrderDto Create(
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
            IEnumerable<ReturnOrderItemDto> items = null)
        {
            var returnOrderId = UlidGenerator.NewUlid();
            var order = new ReturnOrder(
                returnOrderId,
                DateTime.Today,
                custId,
                custName,
                warehouseCode,
                operatorName,
                driverId,
                driverName,
                salesPersonId,
                salesPersonName,
                keterangan,
                StatusSync.Draft,
                customerCode: customerCode,
                wilayah: wilayah);

            PopulateItems(order, items);

            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public async Task<ReturnOrderDto> CreateAsync(
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
            IEnumerable<ReturnOrderItemDto> items = null)
        {
            var returnOrderId = UlidGenerator.NewUlid();
            var order = new ReturnOrder(
                returnOrderId,
                DateTime.Today,
                custId,
                custName,
                warehouseCode,
                operatorName,
                driverId,
                driverName,
                salesPersonId,
                salesPersonName,
                keterangan,
                StatusSync.Draft,
                customerCode: customerCode,
                wilayah: wilayah);

            PopulateItems(order, items);

            await _returnOrderRepo.SaveAsync(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public ReturnOrderDto Update(
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
            IEnumerable<ReturnOrderItemDto> items = null)
        {
            var order = _returnOrderRepo.GetById(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            order.EnsureModifiable("Update");
            order.SetCustomer(custId, custName, customerCode, wilayah);
            order.SetDriver(driverId, driverName);
            order.SetSalesPerson(salesPersonId, salesPersonName);
            order.SetKeterangan(keterangan);

            order.ClearItems();
            PopulateItems(order, items);

            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public async Task<ReturnOrderDto> UpdateAsync(
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
            IEnumerable<ReturnOrderItemDto> items = null)
        {
            var order = await _returnOrderRepo.GetByIdAsync(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            order.EnsureModifiable("Update");
            order.SetCustomer(custId, custName, customerCode, wilayah);
            order.SetDriver(driverId, driverName);
            order.SetSalesPerson(salesPersonId, salesPersonName);
            order.SetKeterangan(keterangan);

            order.ClearItems();
            PopulateItems(order, items);

            await _returnOrderRepo.SaveAsync(order);
            return ReturnOrderDto.FromDomain(order);
        }

        private static void PopulateItems(ReturnOrder order, IEnumerable<ReturnOrderItemDto> items)
        {
            if (items == null) return;

            foreach (var itm in items)
            {
                var jenisEnum = string.Equals(itm.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase)
                    || (Enum.TryParse<JenisRetur>(itm.JenisRetur, true, out var parsed) && parsed == JenisRetur.RUSAK)
                    ? JenisRetur.RUSAK
                    : JenisRetur.BAGUS;

                order.AddItem(
                    itm.BrgId,
                    itm.BrgName,
                    itm.Barcode,
                    jenisEnum,
                    itm.Qty,
                    itm.SatId,
                    itm.Keterangan);
            }
        }

        public ReturnOrderDto AddItem(
            string returnOrderId,
            string brgId,
            string brgName,
            string barcode,
            JenisRetur jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null)
        {
            var order = _returnOrderRepo.GetById(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            order.AddItem(brgId, brgName, barcode, jenisRetur, qty, satId, keterangan);
            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public ReturnOrderDto UpdateLine(
            string returnOrderId,
            int noUrut,
            decimal newQty,
            JenisRetur newJenisRetur,
            string newKeterangan = null)
        {
            var order = _returnOrderRepo.GetById(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            order.UpdateItem(noUrut, newQty, newJenisRetur, newKeterangan);
            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public ReturnOrderDto UpdateLine(
            string returnOrderId,
            int noUrut,
            decimal newQty,
            string newJenisRetur,
            string newKeterangan = null)
        {
            var jenis = Enum.TryParse<JenisRetur>(newJenisRetur, true, out var parsed)
                ? parsed
                : JenisRetur.BAGUS;

            return UpdateLine(returnOrderId, noUrut, newQty, jenis, newKeterangan);
        }

        public ReturnOrderDto RemoveLine(
            string returnOrderId,
            int noUrut)
        {
            var order = _returnOrderRepo.GetById(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            order.RemoveItem(noUrut);
            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public ReturnOrderDto CommitDraft(string returnOrderId)
        {
            var order = _returnOrderRepo.GetById(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            _returnOrderRepo.Save(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public async Task<ReturnOrderDto> CommitDraftAsync(string returnOrderId)
        {
            var order = await _returnOrderRepo.GetByIdAsync(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            await _returnOrderRepo.SaveAsync(order);
            return ReturnOrderDto.FromDomain(order);
        }

        public async Task<SyncResultDto> CommitAndUploadAsync(string returnOrderId)
        {
            var order = await _returnOrderRepo.GetByIdAsync(returnOrderId)
                ?? throw new KeyNotFoundException($"Return Order '{returnOrderId}' not found.");

            await _returnOrderRepo.SaveAsync(order);

            if (_syncService != null)
            {
                return await _syncService.TriggerDocumentUploadAsync(returnOrderId);
            }

            return SyncResultDto.Ok("ReturnOrder", pushed: 1);
        }

        public ReturnOrderDto GetById(string returnOrderId)
        {
            var order = _returnOrderRepo.GetById(returnOrderId);
            return ReturnOrderDto.FromDomain(order);
        }

        public async Task<ReturnOrderDto> GetByIdAsync(string returnOrderId)
        {
            var order = await _returnOrderRepo.GetByIdAsync(returnOrderId);
            return ReturnOrderDto.FromDomain(order);
        }

        public IEnumerable<ReturnOrderDto> GetOrders(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
        {
            var orders = _returnOrderRepo.GetFilteredList(startDate, endDate, statusFilter);
            return orders.Select(ReturnOrderDto.FromDomain);
        }

        public async Task<IEnumerable<ReturnOrderDto>> GetOrdersAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null)
        {
            var orders = await _returnOrderRepo.GetFilteredListAsync(startDate, endDate, statusFilter);
            return orders.Select(ReturnOrderDto.FromDomain);
        }

        public void DeleteDraft(string returnOrderId)
        {
            _returnOrderRepo.DeleteDraft(returnOrderId);
        }

        public async Task DeleteDraftAsync(string returnOrderId)
        {
            await _returnOrderRepo.DeleteDraftAsync(returnOrderId);
        }
    }
}

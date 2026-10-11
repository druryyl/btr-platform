using System;
using System.Collections.Generic;
using System.Linq;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a Return Order document.
    /// Supports bi-directional mapping with domain aggregate root and serialization for Cloud API.
    /// </summary>
    public class ReturnOrderDto
    {
        public string ReturnOrderId { get; set; }
        public DateTime ReturnOrderDate { get; set; } = DateTime.Today;
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string CustomerId => CustId; // Alias for cloud API
        public string CustomerName => CustName; // Alias for cloud API
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string WarehouseCode { get; set; }
        public string OperatorName { get; set; }
        public string Keterangan { get; set; }
        public string Note => Keterangan; // Alias for cloud API
        public string StatusSync { get; set; } = "Draft";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public DateTime? SyncedDate { get; set; }
        public bool IsSynced => string.Equals(StatusSync, "Synced", StringComparison.OrdinalIgnoreCase);

        public decimal TotalQtyBagus { get; set; }
        public decimal TotalQtyRusak { get; set; }
        public int TotalItemsCount { get; set; }

        public List<ReturnOrderItemDto> Items { get; set; } = new List<ReturnOrderItemDto>();
        public IEnumerable<ReturnOrderItemDto> ListItem => Items; // Alias for Cloud API (ReturnOrderUploadCommand)

        public ReturnOrderDto()
        {
        }

        public static ReturnOrderDto FromDomain(ReturnOrder order)
        {
            if (order == null) return null;

            var itemsDto = order.Items.Select(ReturnOrderItemDto.FromDomain).ToList();

            return new ReturnOrderDto
            {
                ReturnOrderId = order.ReturnOrderId,
                ReturnOrderDate = order.ReturnOrderDate,
                CustId = order.CustId,
                CustName = order.CustName,
                DriverId = order.DriverId,
                DriverName = order.DriverName,
                SalesPersonId = order.SalesPersonId,
                SalesPersonName = order.SalesPersonName,
                WarehouseCode = order.WarehouseCode,
                OperatorName = order.OperatorName,
                Keterangan = order.Keterangan,
                StatusSync = order.StatusSync.ToString(),
                CreatedDate = order.CreatedDate,
                ModifiedDate = order.ModifiedDate,
                SyncedDate = order.SyncedDate,
                TotalQtyBagus = order.TotalQtyBagus,
                TotalQtyRusak = order.TotalQtyRusak,
                TotalItemsCount = order.TotalItemsCount,
                Items = itemsDto
            };
        }

        public ReturnOrder ToDomain()
        {
            var status = Enum.TryParse<StatusSync>(StatusSync, true, out var parsed)
                ? parsed
                : Domain.Enums.StatusSync.Draft;

            var domain = new ReturnOrder(
                ReturnOrderId,
                ReturnOrderDate,
                CustId,
                CustName,
                WarehouseCode,
                OperatorName,
                DriverId,
                DriverName,
                SalesPersonId,
                SalesPersonName,
                Keterangan,
                status,
                CreatedDate,
                ModifiedDate,
                SyncedDate);

            if (Items != null)
            {
                foreach (var itemDto in Items)
                {
                    domain.AddItem(
                        itemDto.BrgId,
                        itemDto.BrgName,
                        itemDto.Barcode,
                        Enum.TryParse<JenisRetur>(itemDto.JenisRetur, true, out var jr) ? jr : Domain.Enums.JenisRetur.BAGUS,
                        itemDto.Qty,
                        itemDto.SatId,
                        itemDto.Keterangan);
                }
            }

            return domain;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using BtrReturGudang.Domain.Common;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Aggregate Root representing a Return Order document captured at the warehouse.
    /// Traceable to: RETURN-ORDER-DOMAIN §7, BR-001 through BR-020, TD-004, TD-010.
    /// </summary>
    public class ReturnOrder
    {
        private readonly List<ReturnOrderItem> _items = new List<ReturnOrderItem>();

        public string ReturnOrderId { get; private set; }
        public DateTime ReturnOrderDate { get; private set; }
        public string CustId { get; private set; }
        public string CustName { get; private set; }
        public string CustomerCode { get; private set; }
        public string Wilayah { get; private set; }
        public string CustomerId => CustId;
        public string CustomerName => CustName;
        public string DriverId { get; private set; }
        public string DriverName { get; private set; }
        public string SalesPersonId { get; private set; }
        public string SalesPersonName { get; private set; }
        public string WarehouseCode { get; private set; }
        public string OperatorName { get; private set; }
        public string Keterangan { get; private set; }
        public StatusSync StatusSync { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime ModifiedDate { get; private set; }
        public DateTime? SyncedDate { get; private set; }

        public IReadOnlyList<ReturnOrderItem> Items => _items.AsReadOnly();

        public decimal TotalQtyBagus => _items.Where(i => i.JenisRetur == JenisRetur.BAGUS).Sum(i => i.Qty);
        public decimal TotalQtyRusak => _items.Where(i => i.JenisRetur == JenisRetur.RUSAK).Sum(i => i.Qty);
        public int TotalItemsCount => _items.Count;
        public bool IsSynced => StatusSync == StatusSync.Synced;

        /// <summary>
        /// Parameterless constructor for Dapper and ORM hydration.
        /// </summary>
        public ReturnOrder()
        {
        }

        public ReturnOrder(
            string returnOrderId,
            DateTime returnOrderDate,
            string custId,
            string custName,
            string warehouseCode,
            string operatorName,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            StatusSync statusSync = StatusSync.Draft,
            DateTime? createdDate = null,
            DateTime? modifiedDate = null,
            DateTime? syncedDate = null,
            string customerCode = null,
            string wilayah = null)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                returnOrderId = UlidGenerator.NewUlid();

            if (string.IsNullOrWhiteSpace(custId))
                throw new DomainException("Customer ID (CustId) is mandatory (BR-001, BR-002).");

            if (string.IsNullOrWhiteSpace(custName))
                throw new DomainException("Customer Name (CustName) is mandatory (BR-001, BR-002).");

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new DomainException("Warehouse Code is mandatory (BR-005, BR-006).");

            if (string.IsNullOrWhiteSpace(operatorName))
                throw new DomainException("Operator Name is mandatory.");

            var now = DateTime.Now;

            ReturnOrderId = returnOrderId;
            ReturnOrderDate = returnOrderDate == default ? now.Date : returnOrderDate;
            CustId = custId;
            CustName = custName;
            CustomerCode = customerCode;
            Wilayah = wilayah;
            WarehouseCode = warehouseCode;
            OperatorName = operatorName;
            DriverId = driverId;
            DriverName = driverName;
            SalesPersonId = salesPersonId;
            SalesPersonName = salesPersonName;
            Keterangan = keterangan;
            StatusSync = statusSync;
            CreatedDate = createdDate ?? now;
            ModifiedDate = modifiedDate ?? now;
            SyncedDate = syncedDate;
        }

        /// <summary>
        /// Factory method to create a new draft Return Order.
        /// </summary>
        public static ReturnOrder Create(
            string custId,
            string custName,
            string warehouseCode,
            string operatorName,
            DateTime? returnOrderDate = null,
            string driverId = null,
            string driverName = null,
            string salesPersonId = null,
            string salesPersonName = null,
            string keterangan = null,
            string customerCode = null,
            string wilayah = null)
        {
            return new ReturnOrder(
                UlidGenerator.NewUlid(),
                returnOrderDate ?? DateTime.Today,
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
        }

        /// <summary>
        /// Enforces post-sync immutability (BR-018, BR-020, TD-010).
        /// Throws DocumentLockedException if document is already synced.
        /// </summary>
        public void EnsureModifiable(string action = "Modify")
        {
            if (StatusSync == StatusSync.Synced)
            {
                throw new DocumentLockedException(ReturnOrderId, action);
            }
        }

        public void AddItem(ReturnOrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            EnsureModifiable(nameof(AddItem));

            item.ReturnOrderId = ReturnOrderId;
            item.NoUrut = _items.Count + 1;
            _items.Add(item);
            ModifiedDate = DateTime.Now;
        }

        public void AddItem(
            string brgId,
            string brgName,
            string barcode,
            JenisRetur jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null)
        {
            var item = ReturnOrderItem.Create(brgId, brgName, barcode, jenisRetur, qty, satId, keterangan);
            AddItem(item);
        }

        public void RemoveItem(int noUrut)
        {
            EnsureModifiable(nameof(RemoveItem));

            var item = _items.FirstOrDefault(i => i.NoUrut == noUrut);
            if (item == null)
                throw new DomainException($"Item with NoUrut {noUrut} not found in ReturnOrder '{ReturnOrderId}'.");

            _items.Remove(item);
            ResequenceItems();
            ModifiedDate = DateTime.Now;
        }

        public void UpdateItem(int noUrut, decimal newQty, JenisRetur newJenisRetur, string newKeterangan = null)
        {
            EnsureModifiable(nameof(UpdateItem));

            var item = _items.FirstOrDefault(i => i.NoUrut == noUrut);
            if (item == null)
                throw new DomainException($"Item with NoUrut {noUrut} not found in ReturnOrder '{ReturnOrderId}'.");

            item.Update(newQty, newJenisRetur, newKeterangan);
            ModifiedDate = DateTime.Now;
        }

        public void ClearItems()
        {
            EnsureModifiable(nameof(ClearItems));

            _items.Clear();
            ModifiedDate = DateTime.Now;
        }

        public void SetCustomer(string custId, string custName, string customerCode = null, string wilayah = null)
        {
            EnsureModifiable(nameof(SetCustomer));

            if (string.IsNullOrWhiteSpace(custId))
                throw new DomainException("Customer ID (CustId) is mandatory (BR-001, BR-002).");

            if (string.IsNullOrWhiteSpace(custName))
                throw new DomainException("Customer Name (CustName) is mandatory (BR-001, BR-002).");

            CustId = custId;
            CustName = custName;
            CustomerCode = customerCode;
            Wilayah = wilayah;
            ModifiedDate = DateTime.Now;
        }

        public void SetDriver(string driverId, string driverName)
        {
            EnsureModifiable(nameof(SetDriver));

            DriverId = driverId;
            DriverName = driverName;
            ModifiedDate = DateTime.Now;
        }

        public void SetSalesPerson(string salesPersonId, string salesPersonName)
        {
            EnsureModifiable(nameof(SetSalesPerson));

            SalesPersonId = salesPersonId;
            SalesPersonName = salesPersonName;
            ModifiedDate = DateTime.Now;
        }

        public void SetWarehouse(string warehouseCode)
        {
            EnsureModifiable(nameof(SetWarehouse));

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new DomainException("Warehouse Code is mandatory (BR-005, BR-006).");

            WarehouseCode = warehouseCode;
            ModifiedDate = DateTime.Now;
        }

        public void SetOperator(string operatorName)
        {
            EnsureModifiable(nameof(SetOperator));

            if (string.IsNullOrWhiteSpace(operatorName))
                throw new DomainException("Operator Name is mandatory.");

            OperatorName = operatorName;
            ModifiedDate = DateTime.Now;
        }

        public void SetKeterangan(string keterangan)
        {
            EnsureModifiable(nameof(SetKeterangan));

            Keterangan = keterangan;
            ModifiedDate = DateTime.Now;
        }

        public void SetReturnOrderDate(DateTime date)
        {
            EnsureModifiable(nameof(SetReturnOrderDate));

            ReturnOrderDate = date;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Transitions document from Draft to Synced, stamping SyncedDate and locking further modifications.
        /// Traceable to: BR-018, BR-020, TD-010.
        /// </summary>
        public void MarkSynced(DateTime? syncTime = null)
        {
            Validate();

            StatusSync = StatusSync.Synced;
            SyncedDate = syncTime ?? DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Validates aggregate invariants prior to persistence or synchronization.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ReturnOrderId))
                throw new DomainException("ReturnOrderId is required.");

            if (string.IsNullOrWhiteSpace(CustId) || string.IsNullOrWhiteSpace(CustName))
                throw new DomainException("Customer is mandatory (BR-001, BR-002).");

            if (string.IsNullOrWhiteSpace(WarehouseCode))
                throw new DomainException("Warehouse is mandatory (BR-005, BR-006).");

            if (string.IsNullOrWhiteSpace(OperatorName))
                throw new DomainException("Operator is mandatory.");

            if (_items.Count == 0)
                throw new DomainException("Return Order must contain at least one Return Order Item (BR-012).");
        }

        /// <summary>
        /// Hydrates items collection directly from persistence DAL without triggering mutation events.
        /// </summary>
        public void LoadItems(IEnumerable<ReturnOrderItem> items)
        {
            _items.Clear();
            if (items != null)
            {
                _items.AddRange(items.OrderBy(i => i.NoUrut));
            }
        }

        private void ResequenceItems()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].NoUrut = i + 1;
            }
        }
    }
}

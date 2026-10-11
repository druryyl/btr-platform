using System;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;
using Xunit;

namespace BtrReturGudang.Domain.Tests
{
    public class ReturnOrderImmutabilityTests
    {
        [Fact]
        public void DraftDocument_AllowsMutations()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");

            Assert.Equal(StatusSync.Draft, order.StatusSync);
            Assert.Single(order.Items);
            Assert.Equal(5m, order.TotalQtyBagus);
            Assert.Equal(0m, order.TotalQtyRusak);

            order.AddItem("BRG02", "Indomie Soto", "899654321", JenisRetur.RUSAK, 2m, "Dus");
            Assert.Equal(2, order.Items.Count);
            Assert.Equal(5m, order.TotalQtyBagus);
            Assert.Equal(2m, order.TotalQtyRusak);

            order.UpdateItem(1, 10m, JenisRetur.BAGUS, "Updated qty");
            Assert.Equal(10m, order.Items[0].Qty);

            order.RemoveItem(2);
            Assert.Single(order.Items);
            Assert.Equal(1, order.Items[0].NoUrut);
        }

        [Fact]
        public void SyncedDocument_ThrowsWhenAddingItem()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");
            order.MarkSynced();

            Assert.True(order.IsSynced);
            Assert.NotNull(order.SyncedDate);

            var ex = Assert.Throws<DocumentLockedException>(() =>
                order.AddItem("BRG02", "Indomie Soto", "899654321", JenisRetur.RUSAK, 2m, "Dus"));

            Assert.Contains("locked", ex.Message);
        }

        [Fact]
        public void SyncedDocument_ThrowsWhenRemovingItem()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");
            order.MarkSynced();

            Assert.Throws<DocumentLockedException>(() => order.RemoveItem(1));
        }

        [Fact]
        public void SyncedDocument_ThrowsWhenUpdatingItem()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");
            order.MarkSynced();

            Assert.Throws<DocumentLockedException>(() => order.UpdateItem(1, 20m, JenisRetur.RUSAK));
        }

        [Fact]
        public void SyncedDocument_ThrowsWhenClearingItems()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");
            order.MarkSynced();

            Assert.Throws<DocumentLockedException>(() => order.ClearItems());
        }

        [Fact]
        public void SyncedDocument_ThrowsWhenModifyingHeaderFields()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");
            order.AddItem("BRG01", "Indomie Goreng", "899123456", JenisRetur.BAGUS, 5m, "Dus");
            order.MarkSynced();

            Assert.Throws<DocumentLockedException>(() => order.SetCustomer("CUST02", "Toko Lain"));
            Assert.Throws<DocumentLockedException>(() => order.SetWarehouse("WH-OTHER"));
            Assert.Throws<DocumentLockedException>(() => order.SetOperator("Operator2"));
            Assert.Throws<DocumentLockedException>(() => order.SetDriver("DRV01", "Pak Budi"));
            Assert.Throws<DocumentLockedException>(() => order.SetSalesPerson("SLS01", "Sales 1"));
            Assert.Throws<DocumentLockedException>(() => order.SetKeterangan("Some notes"));
            Assert.Throws<DocumentLockedException>(() => order.SetReturnOrderDate(DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void MarkSynced_RequiresAtLeastOneItem()
        {
            var order = ReturnOrder.Create("CUST01", "Toko Makmur", "WH-MAIN", "Operator1");

            var ex = Assert.Throws<DomainException>(() => order.MarkSynced());
            Assert.Contains("at least one", ex.Message);
        }

        [Fact]
        public void ReturnOrderItem_ValidatesQuantityGreaterThanZero()
        {
            var ex = Assert.Throws<DomainException>(() =>
                ReturnOrderItem.Create("BRG01", "Indomie", "899", JenisRetur.BAGUS, 0m, "Dus"));

            Assert.Contains("greater than zero", ex.Message);
        }
    }
}

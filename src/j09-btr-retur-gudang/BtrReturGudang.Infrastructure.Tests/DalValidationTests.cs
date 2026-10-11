using System;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Infrastructure.Persistence;
using Xunit;

namespace BtrReturGudang.Infrastructure.Tests
{
    public class DalValidationTests
    {
        [Fact]
        public void DalClasses_ImplementExpectedRepositoryInterfaces()
        {
            var returnOrderDal = new ReturnOrderDal();
            var barcodeDal = new BarcodeDal();
            var masterDataDal = new MasterDataDal();
            var syncLogDal = new SyncLogDal();

            Assert.IsAssignableFrom<IReturnOrderRepo>(returnOrderDal);
            Assert.IsAssignableFrom<IBarcodeRepo>(barcodeDal);
            Assert.IsAssignableFrom<IReferenceRepo>(masterDataDal);
            Assert.IsAssignableFrom<ISyncLogRepo>(syncLogDal);
        }

        [Fact]
        public void ReturnOrderDal_Save_NullOrder_ThrowsArgumentNullException()
        {
            var dal = new ReturnOrderDal();
            Assert.Throws<ArgumentNullException>(() => dal.Save(null));
        }

        [Fact]
        public async Task ReturnOrderDal_SaveAsync_NullOrder_ThrowsArgumentNullException()
        {
            var dal = new ReturnOrderDal();
            await Assert.ThrowsAsync<ArgumentNullException>(() => dal.SaveAsync(null));
        }

        [Fact]
        public void ReturnOrderDal_MarkSynced_EmptyId_ThrowsArgumentException()
        {
            var dal = new ReturnOrderDal();
            Assert.Throws<ArgumentException>(() => dal.MarkSynced("", DateTime.Now));
        }

        [Fact]
        public async Task ReturnOrderDal_MarkSyncedAsync_EmptyId_ThrowsArgumentException()
        {
            var dal = new ReturnOrderDal();
            await Assert.ThrowsAsync<ArgumentException>(() => dal.MarkSyncedAsync(null, DateTime.Now));
        }

        [Fact]
        public void ReturnOrderDal_DeleteDraft_EmptyId_ThrowsArgumentException()
        {
            var dal = new ReturnOrderDal();
            Assert.Throws<ArgumentException>(() => dal.DeleteDraft(null));
        }

        [Fact]
        public async Task ReturnOrderDal_DeleteDraftAsync_EmptyId_ThrowsArgumentException()
        {
            var dal = new ReturnOrderDal();
            await Assert.ThrowsAsync<ArgumentException>(() => dal.DeleteDraftAsync(""));
        }

        [Fact]
        public void ReturnOrderDal_GetById_EmptyId_ReturnsNullWithoutDbCall()
        {
            var dal = new ReturnOrderDal();
            var result = dal.GetById(null);
            Assert.Null(result);

            var resultWhitespace = dal.GetById("   ");
            Assert.Null(resultWhitespace);
        }

        [Fact]
        public async Task ReturnOrderDal_GetByIdAsync_EmptyId_ReturnsNullWithoutDbCall()
        {
            var dal = new ReturnOrderDal();
            var result = await dal.GetByIdAsync("");
            Assert.Null(result);
        }

        [Fact]
        public void BarcodeDal_InsertBarcode_Null_ThrowsArgumentNullException()
        {
            var dal = new BarcodeDal();
            Assert.Throws<ArgumentNullException>(() => dal.InsertBarcode(null));
        }

        [Fact]
        public async Task BarcodeDal_InsertBarcodeAsync_Null_ThrowsArgumentNullException()
        {
            var dal = new BarcodeDal();
            await Assert.ThrowsAsync<ArgumentNullException>(() => dal.InsertBarcodeAsync(null));
        }

        [Fact]
        public void BarcodeDal_QueueRegistration_Null_ThrowsArgumentNullException()
        {
            var dal = new BarcodeDal();
            Assert.Throws<ArgumentNullException>(() => dal.QueueRegistration(null));
        }

        [Fact]
        public async Task BarcodeDal_QueueRegistrationAsync_Null_ThrowsArgumentNullException()
        {
            var dal = new BarcodeDal();
            await Assert.ThrowsAsync<ArgumentNullException>(() => dal.QueueRegistrationAsync(null));
        }

        [Fact]
        public void BarcodeDal_MarkRegistrationUploaded_Empty_ThrowsArgumentException()
        {
            var dal = new BarcodeDal();
            Assert.Throws<ArgumentException>(() => dal.MarkRegistrationUploaded(null));
        }

        [Fact]
        public void BarcodeDal_LookupByBarcode_Empty_ReturnsNullWithoutDbCall()
        {
            var dal = new BarcodeDal();
            Assert.Null(dal.LookupByBarcode(""));
            Assert.Null(dal.LookupByBarcode("   \r\n\t"));
        }

        [Fact]
        public async Task BarcodeDal_LookupByBarcodeAsync_Empty_ReturnsNullWithoutDbCall()
        {
            var dal = new BarcodeDal();
            Assert.Null(await dal.LookupByBarcodeAsync(""));
        }

        [Fact]
        public void MasterDataDal_GetBarangById_Empty_ReturnsNullWithoutDbCall()
        {
            var dal = new MasterDataDal();
            Assert.Null(dal.GetBarangById(null));
            Assert.Null(dal.GetBarangById("  "));
        }

        [Fact]
        public void MasterDataDal_GetCustomerById_Empty_ReturnsNullWithoutDbCall()
        {
            var dal = new MasterDataDal();
            Assert.Null(dal.GetCustomerById(null));
            Assert.Null(dal.GetCustomerById(""));
        }

        [Fact]
        public void SyncLogDal_LogSync_Null_ThrowsArgumentNullException()
        {
            var dal = new SyncLogDal();
            Assert.Throws<ArgumentNullException>(() => dal.LogSync(null));
        }

        [Fact]
        public async Task SyncLogDal_LogSyncAsync_Null_ThrowsArgumentNullException()
        {
            var dal = new SyncLogDal();
            await Assert.ThrowsAsync<ArgumentNullException>(() => dal.LogSyncAsync(null));
        }

        [Fact]
        public void SyncLogDal_GetLastSuccessfulSyncTimestamp_Empty_ReturnsNullWithoutDbCall()
        {
            var dal = new SyncLogDal();
            Assert.Null(dal.GetLastSuccessfulSyncTimestamp(null));
            Assert.Null(dal.GetLastSuccessfulSyncTimestamp(""));
        }
    }
}

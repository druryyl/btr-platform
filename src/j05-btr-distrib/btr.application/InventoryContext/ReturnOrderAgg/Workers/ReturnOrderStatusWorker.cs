using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.InventoryContext.ReturnOrderAgg.Workers
{
    public interface IReturnOrderStatusWorker : INunaService<string, string>
    {
        string UpdateStatus(string returnOrderId, SqlConnection conn = null, SqlTransaction trans = null);
    }

    public class ReturnOrderStatusWorker : IReturnOrderStatusWorker
    {
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;
        private readonly IReturJualDal _returJualDal;
        private readonly DateTimeProvider _dateTimeProvider;

        public ReturnOrderStatusWorker(
            IReturnOrderDal returnOrderDal,
            IReturnOrderItemDal returnOrderItemDal,
            IReturJualDal returJualDal,
            DateTimeProvider dateTimeProvider = null)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
            _returJualDal = returJualDal;
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        public string Execute(string req) => UpdateStatus(req);

        public string UpdateStatus(string returnOrderId, SqlConnection conn = null, SqlTransaction trans = null)
        {
            if (string.IsNullOrWhiteSpace(returnOrderId))
                return string.Empty;

            var returnOrder = _returnOrderDal.GetData(new ReturnOrderModel(returnOrderId));
            if (returnOrder == null)
                return string.Empty;

            // 1. Ambil distinct JenisRetur dari detail item order retur
            var orderItems = _returnOrderItemDal.ListData(new ReturnOrderModel(returnOrderId))
                ?? Enumerable.Empty<ReturnOrderItemModel>();
            var requiredJenisRetur = orderItems
                .Select(x => (x.JenisRetur ?? string.Empty).Trim().ToUpper())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            // 2. Ambil seluruh Retur Jual yang merujuk pada ReturnOrderId tersebut
            var returJualList = (_returJualDal.ListByReturnOrderId(returnOrderId)
                ?? Enumerable.Empty<ReturJualModel>()).ToList();

            // 3. Filter dokumen Retur Jual aktif (VoidDate = 3000-01-01 / !IsVoid)
            var activeReturJualList = returJualList
                .Where(x => !x.IsVoid)
                .ToList();

            var activeJenisRetur = activeReturJualList
                .Select(x => (x.JenisRetur ?? string.Empty).Trim().ToUpper())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            // 4. Evaluasi status ReturnOrder secara deterministik dua arah
            string newStatus;
            if (activeReturJualList.Count == 0)
            {
                newStatus = ReturnOrderStatus.Synced;
            }
            else if (requiredJenisRetur.Count > 0)
            {
                if (requiredJenisRetur.All(j => activeJenisRetur.Contains(j)))
                    newStatus = ReturnOrderStatus.Imported;
                else
                    newStatus = ReturnOrderStatus.Partial;
            }
            else
            {
                newStatus = ReturnOrderStatus.Imported;
            }

            // 5. Sinkronisasi data SalesPersonId dan DriverId jika di ReturnOrder masih kosong
            if (string.IsNullOrWhiteSpace(returnOrder.SalesPersonId))
            {
                var rjWithSales = activeReturJualList.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SalesPersonId))
                    ?? returJualList.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SalesPersonId));
                if (rjWithSales != null)
                {
                    returnOrder.SalesPersonId = rjWithSales.SalesPersonId;
                    if (!string.IsNullOrWhiteSpace(rjWithSales.SalesPersonName))
                        returnOrder.SalesPersonName = rjWithSales.SalesPersonName;
                }
            }

            if (string.IsNullOrWhiteSpace(returnOrder.DriverId))
            {
                var rjWithDriver = activeReturJualList.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.DriverId))
                    ?? returJualList.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.DriverId));
                if (rjWithDriver != null)
                {
                    returnOrder.DriverId = rjWithDriver.DriverId;
                    if (!string.IsNullOrWhiteSpace(rjWithDriver.DriverName))
                        returnOrder.DriverName = rjWithDriver.DriverName;
                }
            }

            returnOrder.Status = newStatus;
            returnOrder.ModifiedDate = _dateTimeProvider?.Now ?? DateTime.Now;

            // 6. Persist perubahan (mendukung ambient transaction scope atau explicit connection)
            if (conn != null)
            {
                using (var cmd = conn.CreateCommand())
                {
                    if (trans != null)
                        cmd.Transaction = trans;

                    cmd.CommandText = @"
                        UPDATE 
                            BTR_ReturnOrder 
                        SET 
                            ReturnOrderNo = @ReturnOrderNo,
                            ReturnOrderDate = @ReturnOrderDate,
                            WarehouseCode = @WarehouseCode,
                            CustomerId = @CustomerId,
                            SalesPersonId = @SalesPersonId,
                            DriverId = @DriverId,
                            Note = @Note,
                            Status = @Status,
                            ModifiedBy = @ModifiedBy,
                            ModifiedDate = @ModifiedDate
                        WHERE 
                            ReturnOrderId = @ReturnOrderId";

                    cmd.Parameters.Add(new SqlParameter("@ReturnOrderId", SqlDbType.VarChar) { Value = returnOrder.ReturnOrderId ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ReturnOrderNo", SqlDbType.VarChar) { Value = returnOrder.ReturnOrderNo ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ReturnOrderDate", SqlDbType.DateTime) { Value = returnOrder.ReturnOrderDate });
                    cmd.Parameters.Add(new SqlParameter("@WarehouseCode", SqlDbType.VarChar) { Value = returnOrder.WarehouseCode ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.VarChar) { Value = returnOrder.CustomerId ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@SalesPersonId", SqlDbType.VarChar) { Value = returnOrder.SalesPersonId ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@DriverId", SqlDbType.VarChar) { Value = returnOrder.DriverId ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@Note", SqlDbType.VarChar) { Value = returnOrder.Note ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar) { Value = returnOrder.Status ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.VarChar) { Value = returnOrder.ModifiedBy ?? string.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime) { Value = returnOrder.ModifiedDate });

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                _returnOrderDal.Update(returnOrder);
            }

            return newStatus;
        }
    }
}

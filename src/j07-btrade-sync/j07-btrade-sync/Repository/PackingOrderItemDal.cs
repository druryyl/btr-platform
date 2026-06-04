using btr.domain.InventoryContext.PackingOrderFeature;
using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Repository
{
    public class PackingOrderItemDal 
    {
        public void Insert(IEnumerable<PackingOrderItemDto> listDto)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            using (var bcp = new SqlBulkCopy(conn))
            {
                conn.Open();
                bcp.AddMap("PackingOrderId", "PackingOrderId");
                bcp.AddMap("NoUrut", "NoUrut");
                bcp.AddMap("BrgId", "BrgId");
                bcp.AddMap("BrgCode", "BrgCode");
                bcp.AddMap("BrgName", "BrgName");
                bcp.AddMap("KategoriName", "KategoriName");
                bcp.AddMap("SupplierName", "SupplierName");
                bcp.AddMap("QtyBesar", "QtyBesar");
                bcp.AddMap("SatBesar", "SatBesar");
                bcp.AddMap("QtyKecil", "QtyKecil");
                bcp.AddMap("SatKecil", "SatKecil");
                bcp.AddMap("DepoId", "DepoId");

                var fetched = listDto.ToList();
                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "BTR_PackingOrderItem";
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(IPackingOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTR_PackingOrderItem
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public IEnumerable<PackingOrderItemDto> ListData(IPackingOrderKey filter)
        {
            const string sql = @"
                SELECT
                    aa.PackingOrderId, aa.NoUrut, 
                    aa.BrgId, aa.BrgCode, aa.BrgName,
                    aa.KategoriName, aa.SupplierName,       
                    aa.QtyBesar, aa.SatBesar,
                    aa.QtyKecil, aa.SatKecil,
                    aa.DepoId
                FROM BTR_PackingOrderItem aa
                LEFT JOIN BTR_Depo bb ON aa.DepoId = bb.DepoId  
                WHERE PackingOrderId = @PackingOrderId
                ORDER BY NoUrut
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", filter.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.Read<PackingOrderItemDto>(sql, dp);
            }
        }
    }

    public class PackingOrderItemDto
    {
        public PackingOrderItemDto(string packingOrderId, int noUrut, string brgId, string brgCode, string brgName,
            string kategoriName, string supplierName,
            int qtyBesar, string satBesar, int qtyKecil, string satKecil, string depoId)
        {
            PackingOrderId = packingOrderId;
            NoUrut = noUrut;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            KategoriName = kategoriName;
            SupplierName = supplierName;
            QtyBesar = qtyBesar;
            SatBesar = satBesar;
            QtyKecil = qtyKecil;
            SatKecil = satKecil;
            DepoId = depoId;
        }
        public string PackingOrderId { get; private set; }
        public int NoUrut { get; private set; }
        public string BrgId { get; private set; }
        public string BrgCode { get; private set; }
        public string BrgName { get; private set; }
        public string KategoriName { get; private set; }
        public string SupplierName { get; private set; }

        public int QtyBesar { get; private set; }
        public string SatBesar { get; private set; }
        public int QtyKecil { get; private set; }
        public string SatKecil { get; private set; }

        public string DepoId { get; private set; }

        public PackingOrderItemModel ToModel()
        {
            var result = new PackingOrderItemModel(
                NoUrut, BrgId, BrgCode, BrgName, 
                KategoriName, SupplierName, 
                QtyKecil, SatKecil, QtyBesar, SatBesar, DepoId);
            return result;
        }
    }
}


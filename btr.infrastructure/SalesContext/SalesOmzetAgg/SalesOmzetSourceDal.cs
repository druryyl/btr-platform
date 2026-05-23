using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturAgg.Contracts;
using btr.application.SalesContext.FakturControlAgg;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.FakturAgg;
using btr.domain.SalesContext.FakturControlAgg;
using btr.domain.SalesContext.OrderAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetSourceDal : ISalesOmzetSourceDal
    {
        private readonly IOrderDal _orderDal;
        private readonly IFakturDal _fakturDal;
        private readonly ICustomerDal _customerDal;
        private readonly IFakturControlStatusDal _controlStatusDal;
        private readonly DatabaseOptions _opt;

        public SalesOmzetSourceDal(
            IOrderDal orderDal,
            IFakturDal fakturDal,
            ICustomerDal customerDal,
            IFakturControlStatusDal controlStatusDal,
            IOptions<DatabaseOptions> opt)
        {
            _orderDal = orderDal;
            _fakturDal = fakturDal;
            _customerDal = customerDal;
            _controlStatusDal = controlStatusDal;
            _opt = opt.Value;
        }

        public IEnumerable<OrderSnapshot> ListOrders(Periode periode) =>
            _orderDal.ListData(periode).Select(MapOrder).Where(o => o != null);

        public IEnumerable<FakturSnapshot> ListFakturs(Periode periode) =>
            _fakturDal.ListData(periode).Select(MapFaktur);

        public OrderSnapshot GetOrderByOrderId(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return null;
            var model = _orderDal.GetData(OrderModel.Key(orderId));
            return MapOrder(model);
        }

        public FakturSnapshot GetFakturByFakturId(string fakturId)
        {
            if (string.IsNullOrEmpty(fakturId)) return null;
            IFakturKey key = new FakturModel(fakturId);
            var model = _fakturDal.GetData(key);
            return model is null ? null : MapFaktur(model);
        }

        public FakturSnapshot GetFakturByOrderId(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return null;

            const string sql = @"
            SELECT TOP 1
                aa.FakturId, aa.FakturDate, aa.FakturCode,
                aa.OrderId, aa.GrandTotal, aa.CustomerId, aa.VoidDate,
                ISNULL(bb.SalesPersonName, '') AS SalesPersonName
            FROM BTR_Faktur aa
                LEFT JOIN BTR_SalesPerson bb ON aa.SalesPersonId = bb.SalesPersonId
            WHERE aa.OrderId = @OrderId
                AND aa.VoidDate = '3000-01-01'
            ORDER BY aa.FakturDate DESC, aa.FakturId DESC";

            var dp = new DynamicParameters();
            dp.AddParam("@OrderId", orderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var row = conn.ReadSingle<FakturRow>(sql, dp);
                return row is null ? null : MapFaktur(row);
            }
        }

        public CustomerSnapshot GetCustomer(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return null;
            var model = _customerDal.GetData(new CustomerModel(customerId));
            return model is null
                ? null
                : new CustomerSnapshot
                {
                    CustomerId = model.CustomerId,
                    CustomerName = model.CustomerName,
                    CustomerCode = model.CustomerCode,
                    Address1 = model.Address1
                };
        }

        public IEnumerable<FakturControlStatusSnapshot> ListControlStatus(string fakturId)
        {
            if (string.IsNullOrEmpty(fakturId))
                return Enumerable.Empty<FakturControlStatusSnapshot>();

            return _controlStatusDal
                .ListData(new FakturModel(fakturId))
                .Select(s => new FakturControlStatusSnapshot
                {
                    FakturId = s.FakturId,
                    StatusFaktur = s.StatusFaktur,
                    StatusDate = s.StatusDate
                });
        }

        private static OrderSnapshot MapOrder(OrderModel model)
        {
            if (model is null) return null;

            return new OrderSnapshot
            {
                OrderId = model.OrderId,
                OrderDate = ParseOrderDate(model.OrderDate),
                OrderTotal = model.TotalAmount,
                SalesName = model.SalesName,
                UserEmail = model.UserEmail,
                CustomerId = model.CustomerId,
                CustomerName = model.CustomerName,
                StatusSync = model.StatusSync
            };
        }

        private static FakturSnapshot MapFaktur(FakturModel model) => new FakturSnapshot
        {
            FakturId = model.FakturId,
            FakturCode = model.FakturCode,
            FakturDate = model.FakturDate,
            OrderId = model.OrderId,
            FakturTotal = model.GrandTotal,
            SalesPersonName = model.SalesPersonName,
            CustomerId = model.CustomerId,
            VoidDate = model.VoidDate
        };

        private static FakturSnapshot MapFaktur(FakturRow row) => new FakturSnapshot
        {
            FakturId = row.FakturId,
            FakturCode = row.FakturCode,
            FakturDate = row.FakturDate,
            OrderId = row.OrderId,
            FakturTotal = row.GrandTotal,
            SalesPersonName = row.SalesPersonName,
            CustomerId = row.CustomerId,
            VoidDate = row.VoidDate
        };

        private static DateTime ParseOrderDate(string orderDate)
        {
            if (string.IsNullOrWhiteSpace(orderDate))
                return SalesOmzetDates.Sentinel;

            if (DateTime.TryParse(orderDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            if (DateTime.TryParse(orderDate, out parsed))
                return parsed;

            return SalesOmzetDates.Sentinel;
        }

        private class FakturRow
        {
            public string FakturId { get; set; }
            public DateTime FakturDate { get; set; }
            public string FakturCode { get; set; }
            public string OrderId { get; set; }
            public decimal GrandTotal { get; set; }
            public string CustomerId { get; set; }
            public DateTime VoidDate { get; set; }
            public string SalesPersonName { get; set; }
        }
    }
}

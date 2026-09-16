using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.BrgContext.BrgAgg;
using btr.application.InventoryContext.DriverAgg;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.InventoryContext.DriverAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.InventoryContext.ReturnOrderAgg.Workers
{
    public interface IReturnOrderBuilder : INunaBuilder<ReturnOrderModel>
    {
        IReturnOrderBuilder Load(IReturnOrderKey returnOrderKey);
        IReturnOrderBuilder Create();
        IReturnOrderBuilder Attach(ReturnOrderModel model);
        IReturnOrderBuilder Customer(ICustomerKey customerKey);
        IReturnOrderBuilder Warehouse(IWarehouseKey warehouseKey);
        IReturnOrderBuilder SalesPerson(ISalesPersonKey salesPersonKey);
        IReturnOrderBuilder Driver(IDriverKey driverKey);
        IReturnOrderBuilder ReturnOrderDate(DateTime returnOrderDate);
        IReturnOrderBuilder AddItem(ReturnOrderItemModel item);
        IReturnOrderBuilder Note(string note);
    }

    public class ReturnOrderBuilder : IReturnOrderBuilder
    {
        private ReturnOrderModel _aggregate;
        private readonly IReturnOrderDal _returnOrderDal;
        private readonly IReturnOrderItemDal _returnOrderItemDal;

        private readonly ICustomerDal _customerDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly IDriverDal _driverDal;
        private readonly IBrgDal _brgDal;

        public ReturnOrderBuilder(IReturnOrderDal returnOrderDal,
            IReturnOrderItemDal returnOrderItemDal,
            ICustomerDal customerDal,
            IWarehouseDal warehouseDal,
            ISalesPersonDal salesPersonDal,
            IDriverDal driverDal,
            IBrgDal brgDal)
        {
            _returnOrderDal = returnOrderDal;
            _returnOrderItemDal = returnOrderItemDal;
            _customerDal = customerDal;
            _warehouseDal = warehouseDal;
            _salesPersonDal = salesPersonDal;
            _driverDal = driverDal;
            _brgDal = brgDal;
        }

        public IReturnOrderBuilder Load(IReturnOrderKey returnOrderKey)
        {
            _aggregate = _returnOrderDal.GetData(returnOrderKey)
                ?? throw new Exception("Return Order tidak ditemukan");

            _aggregate.ListItem = _returnOrderItemDal.ListData(returnOrderKey)?.ToList()
                                  ?? new List<ReturnOrderItemModel>();

            ResolveWarehouseName();
            ResolveCustomerName();
            ResolveSalesPersonName();
            ResolveDriverName();
            ResolveItemBrg();
            return this;
        }

        public IReturnOrderBuilder Create()
        {
            _aggregate = new ReturnOrderModel
            {
                ReturnOrderDate = new DateTime(3000, 1, 1),
                ListItem = new List<ReturnOrderItemModel>()
            };
            return this;
        }

        public IReturnOrderBuilder Attach(ReturnOrderModel model)
        {
            _aggregate = model;
            return this;
        }

        public IReturnOrderBuilder Customer(ICustomerKey customerKey)
        {
            var customer = _customerDal.GetData(customerKey)
                ?? throw new KeyNotFoundException("Customer tidak ditemukan");
            _aggregate.CustomerId = customer.CustomerId;
            _aggregate.CustomerName = customer.CustomerName;
            return this;
        }

        public IReturnOrderBuilder Warehouse(IWarehouseKey warehouseKey)
        {
            var warehouse = _warehouseDal.GetData(warehouseKey)
                ?? throw new KeyNotFoundException("Warehouse tidak ditemukan");
            _aggregate.WarehouseCode = warehouse.WarehouseId;
            _aggregate.WarehouseName = warehouse.WarehouseName;
            return this;
        }

        public IReturnOrderBuilder SalesPerson(ISalesPersonKey salesPersonKey)
        {
            var salesPerson = _salesPersonDal.GetData(salesPersonKey)
                ?? throw new KeyNotFoundException("Sales Person tidak ditemukan");
            _aggregate.SalesPersonId = salesPerson.SalesPersonId;
            _aggregate.SalesPersonName = salesPerson.SalesPersonName;
            return this;
        }

        public IReturnOrderBuilder Driver(IDriverKey driverKey)
        {
            var driver = _driverDal.GetData(driverKey)
                ?? throw new KeyNotFoundException("Driver tidak ditemukan");
            _aggregate.DriverId = driver.DriverId;
            _aggregate.DriverName = driver.DriverName;
            return this;
        }

        public IReturnOrderBuilder ReturnOrderDate(DateTime returnOrderDate)
        {
            _aggregate.ReturnOrderDate = returnOrderDate;
            return this;
        }

        public IReturnOrderBuilder AddItem(ReturnOrderItemModel item)
        {
            var noUrut = _aggregate.ListItem
                .DefaultIfEmpty(new ReturnOrderItemModel { NoUrut = 0 })
                .Max(x => x.NoUrut);
            noUrut++;
            item.NoUrut = noUrut;

            _aggregate.ListItem.Add(item);
            return this;
        }

        public IReturnOrderBuilder Note(string note)
        {
            _aggregate.Note = note ?? string.Empty;
            return this;
        }

        public ReturnOrderModel Build()
        {
            _aggregate.RemoveNull();
            return _aggregate;
        }

        private void ResolveWarehouseName()
        {
            if (!string.IsNullOrWhiteSpace(_aggregate.WarehouseName) ||
                string.IsNullOrWhiteSpace(_aggregate.WarehouseCode))
                return;
            _aggregate.WarehouseName =
                _warehouseDal.GetData(new WarehouseModel(_aggregate.WarehouseCode))?.WarehouseName
                ?? string.Empty;
        }

        private void ResolveCustomerName()
        {
            if (!string.IsNullOrWhiteSpace(_aggregate.CustomerName) ||
                string.IsNullOrWhiteSpace(_aggregate.CustomerId))
                return;
            _aggregate.CustomerName =
                _customerDal.GetData(new CustomerModel(_aggregate.CustomerId))?.CustomerName
                ?? string.Empty;
        }

        private void ResolveSalesPersonName()
        {
            if (!string.IsNullOrWhiteSpace(_aggregate.SalesPersonName) ||
                string.IsNullOrWhiteSpace(_aggregate.SalesPersonId))
                return;
            _aggregate.SalesPersonName =
                _salesPersonDal.GetData(new SalesPersonModel(_aggregate.SalesPersonId))?.SalesPersonName
                ?? string.Empty;
        }

        private void ResolveDriverName()
        {
            if (!string.IsNullOrWhiteSpace(_aggregate.DriverName) ||
                string.IsNullOrWhiteSpace(_aggregate.DriverId))
                return;
            _aggregate.DriverName =
                _driverDal.GetData(new DriverModel(_aggregate.DriverId))?.DriverName
                ?? string.Empty;
        }

        private void ResolveItemBrg()
        {
            foreach (var item in _aggregate.ListItem)
            {
                if (string.IsNullOrWhiteSpace(item.BrgId))
                    continue;
                if (!string.IsNullOrWhiteSpace(item.BrgCode) &&
                    !string.IsNullOrWhiteSpace(item.BrgName))
                    continue;

                var brg = _brgDal.GetData(new BrgModel(item.BrgId));
                if (brg is null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.BrgCode))
                    item.BrgCode = brg.BrgCode;
                if (string.IsNullOrWhiteSpace(item.BrgName))
                    item.BrgName = brg.BrgName;
            }
        }
    }
}

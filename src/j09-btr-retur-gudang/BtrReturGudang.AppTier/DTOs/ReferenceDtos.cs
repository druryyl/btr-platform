using System;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.AppTier.DTOs
{
    public class BarangDto
    {
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan1 { get; set; }
        public string Satuan2 { get; set; }
        public string Satuan3 { get; set; }
        public bool IsActive { get; set; } = true;

        public static BarangDto FromDomain(Barang barang)
        {
            if (barang == null) return null;
            return new BarangDto
            {
                BrgId = barang.BrgId,
                BrgName = barang.BrgName,
                Satuan1 = barang.Satuan1,
                Satuan2 = barang.Satuan2,
                Satuan3 = barang.Satuan3,
                IsActive = barang.IsActive
            };
        }

        public Barang ToDomain()
        {
            return new Barang(BrgId, BrgName, Satuan1, Satuan2, Satuan3, IsActive);
        }
    }

    public class BarcodeDto
    {
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public decimal Pengali { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;

        public static BarcodeDto FromDomain(Barcode barcode)
        {
            if (barcode == null) return null;
            return new BarcodeDto
            {
                BarcodeValue = barcode.BarcodeValue,
                BrgId = barcode.BrgId,
                BrgName = barcode.BrgName,
                Satuan = barcode.Satuan,
                Pengali = barcode.Pengali,
                IsActive = barcode.IsActive
            };
        }

        public Barcode ToDomain()
        {
            return new Barcode(BarcodeValue, BrgId, BrgName, Satuan, Pengali, IsActive);
        }
    }

    public class CustomerDto
    {
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string Alamat { get; set; }

        public static CustomerDto FromDomain(Customer customer)
        {
            if (customer == null) return null;
            return new CustomerDto
            {
                CustId = customer.CustId,
                CustName = customer.CustName,
                Alamat = customer.Alamat
            };
        }

        public Customer ToDomain()
        {
            return new Customer(CustId, CustName, Alamat);
        }
    }

    public class DriverDto
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }

        public static DriverDto FromDomain(Driver driver)
        {
            if (driver == null) return null;
            return new DriverDto
            {
                DriverId = driver.DriverId,
                DriverName = driver.DriverName
            };
        }

        public Driver ToDomain()
        {
            return new Driver(DriverId, DriverName);
        }
    }

    public class SalesPersonDto
    {
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }

        public static SalesPersonDto FromDomain(SalesPerson sp)
        {
            if (sp == null) return null;
            return new SalesPersonDto
            {
                SalesPersonId = sp.SalesPersonId,
                SalesPersonName = sp.SalesPersonName
            };
        }

        public SalesPerson ToDomain()
        {
            return new SalesPerson(SalesPersonId, SalesPersonName);
        }
    }
}

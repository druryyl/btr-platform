using System;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.AppTier.DTOs
{
    public class BarangDto
    {
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string SatBesar { get; set; }
        public string SatKecil { get; set; }
        public int Konversi { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public static BarangDto FromDomain(Barang barang)
        {
            if (barang == null) return null;
            return new BarangDto
            {
                BrgId = barang.BrgId,
                BrgName = barang.BrgName,
                SatBesar = barang.SatBesar,
                SatKecil = barang.SatKecil,
                Konversi = barang.Konversi,
                IsActive = barang.IsActive
            };
        }

        public Barang ToDomain()
        {
            return new Barang(BrgId, BrgName, SatBesar, SatKecil, Konversi, IsActive);
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
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Alamat { get; set; }
        public string Wilayah { get; set; }

        public string CustId
        {
            get => CustomerId;
            set => CustomerId = value;
        }

        public string CustName
        {
            get => CustomerName;
            set => CustomerName = value;
        }

        public string CustCode
        {
            get => CustomerCode;
            set => CustomerCode = value;
        }

        public static CustomerDto FromDomain(Customer customer)
        {
            if (customer == null) return null;
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                Alamat = customer.Alamat,
                Wilayah = customer.Wilayah
            };
        }

        public Customer ToDomain()
        {
            return new Customer(CustomerId, CustomerName, Alamat, CustomerCode, Wilayah);
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

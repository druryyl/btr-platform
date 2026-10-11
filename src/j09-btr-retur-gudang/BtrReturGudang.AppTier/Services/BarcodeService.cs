using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.ValueObjects;

namespace BtrReturGudang.AppTier.Services
{
    /// <summary>
    /// Application service managing barcode normalization, lookups, and on-the-spot registrations.
    /// Traceable to: ARCHITECTURE §5.1, §6, P1-S03, P3-S09.
    /// </summary>
    public class BarcodeService : IBarcodeService
    {
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;

        public BarcodeService(
            IBarcodeRepo barcodeRepo,
            IReferenceRepo referenceRepo = null)
        {
            _barcodeRepo = barcodeRepo ?? throw new ArgumentNullException(nameof(barcodeRepo));
            _referenceRepo = referenceRepo;
        }

        public BarcodeLookupResult LookupBarcode(string rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode))
                return BarcodeLookupResult.Miss(rawBarcode, string.Empty);

            var normalized = BarcodeNormalization.Normalize(rawBarcode);
            if (string.IsNullOrWhiteSpace(normalized))
                return BarcodeLookupResult.Miss(rawBarcode, string.Empty);

            // 1. Check local Barcode table
            var barcodeEntity = _barcodeRepo.LookupByBarcode(normalized);
            if (barcodeEntity != null)
            {
                return BarcodeLookupResult.Hit(
                    rawBarcode,
                    normalized,
                    barcodeEntity.BrgId,
                    barcodeEntity.BrgName,
                    barcodeEntity.Satuan,
                    barcodeEntity.Pengali,
                    barcodeEntity.IsActive);
            }

            // 2. Fallback check: check if input matches Item ID (BrgId) directly
            if (_referenceRepo != null)
            {
                var barang = _referenceRepo.GetBarangById(normalized);
                if (barang != null)
                {
                    return BarcodeLookupResult.Hit(
                        rawBarcode,
                        normalized,
                        barang.BrgId,
                        barang.BrgName,
                        barang.Satuan1 ?? "PCS",
                        1.0m,
                        barang.IsActive);
                }
            }

            return BarcodeLookupResult.Miss(rawBarcode, normalized);
        }

        public async Task<BarcodeLookupResult> LookupBarcodeAsync(string rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode))
                return BarcodeLookupResult.Miss(rawBarcode, string.Empty);

            var normalized = BarcodeNormalization.Normalize(rawBarcode);
            if (string.IsNullOrWhiteSpace(normalized))
                return BarcodeLookupResult.Miss(rawBarcode, string.Empty);

            var barcodeEntity = await _barcodeRepo.LookupByBarcodeAsync(normalized);
            if (barcodeEntity != null)
            {
                return BarcodeLookupResult.Hit(
                    rawBarcode,
                    normalized,
                    barcodeEntity.BrgId,
                    barcodeEntity.BrgName,
                    barcodeEntity.Satuan,
                    barcodeEntity.Pengali,
                    barcodeEntity.IsActive);
            }

            if (_referenceRepo != null)
            {
                var barang = await _referenceRepo.GetBarangByIdAsync(normalized);
                if (barang != null)
                {
                    return BarcodeLookupResult.Hit(
                        rawBarcode,
                        normalized,
                        barang.BrgId,
                        barang.BrgName,
                        barang.Satuan1 ?? "PCS",
                        1.0m,
                        barang.IsActive);
                }
            }

            return BarcodeLookupResult.Miss(rawBarcode, normalized);
        }

        public BarcodeRegistrationDto QuickRegisterUnmappedBarcode(
            string rawBarcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            decimal pengali = 1.0m)
        {
            var normalized = BarcodeNormalization.Normalize(rawBarcode);

            // Fetch product name if reference repo is present
            string brgName = null;
            if (_referenceRepo != null)
            {
                var barang = _referenceRepo.GetBarangById(brgId);
                brgName = barang?.BrgName;
            }

            // 1. Create registration request
            var registration = BarcodeRegistration.Create(
                normalized,
                brgId,
                satuan,
                warehouseCode,
                operatorName);

            // 2. Insert into local active barcode table so current intake session succeeds immediately
            var localBarcode = new Barcode(
                normalized,
                brgId,
                brgName ?? string.Empty,
                satuan,
                pengali,
                true);

            _barcodeRepo.InsertBarcode(localBarcode);
            _barcodeRepo.QueueRegistration(registration);

            return BarcodeRegistrationDto.FromDomain(registration, brgName, pengali);
        }

        public async Task<BarcodeRegistrationDto> QuickRegisterUnmappedBarcodeAsync(
            string rawBarcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            decimal pengali = 1.0m)
        {
            var normalized = BarcodeNormalization.Normalize(rawBarcode);

            string brgName = null;
            if (_referenceRepo != null)
            {
                var barang = await _referenceRepo.GetBarangByIdAsync(brgId);
                brgName = barang?.BrgName;
            }

            var registration = BarcodeRegistration.Create(
                normalized,
                brgId,
                satuan,
                warehouseCode,
                operatorName);

            var localBarcode = new Barcode(
                normalized,
                brgId,
                brgName ?? string.Empty,
                satuan,
                pengali,
                true);

            await _barcodeRepo.InsertBarcodeAsync(localBarcode);
            await _barcodeRepo.QueueRegistrationAsync(registration);

            return BarcodeRegistrationDto.FromDomain(registration, brgName, pengali);
        }

        public IEnumerable<BarcodeDto> SearchBarcodes(string keyword)
        {
            return _barcodeRepo.SearchBarcodes(keyword).Select(BarcodeDto.FromDomain);
        }

        public async Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword)
        {
            var barcodes = await _barcodeRepo.SearchBarcodesAsync(keyword);
            return barcodes.Select(BarcodeDto.FromDomain);
        }

        public IEnumerable<BarcodeRegistrationDto> GetPendingRegistrations()
        {
            return _barcodeRepo.GetPendingRegistrations().Select(r => BarcodeRegistrationDto.FromDomain(r));
        }

        public async Task<IEnumerable<BarcodeRegistrationDto>> GetPendingRegistrationsAsync()
        {
            var registrations = await _barcodeRepo.GetPendingRegistrationsAsync();
            return registrations.Select(r => BarcodeRegistrationDto.FromDomain(r));
        }
    }
}

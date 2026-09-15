using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public interface IBrgBarcodeBuilder : INunaBuilder<BrgBarcodeModel>
    {
        IBrgBarcodeBuilder Create();
        IBrgBarcodeBuilder Load(IBrgBarcodeKey brgBarcodeKey);
        IBrgBarcodeBuilder Attach(BrgBarcodeModel brgBarcode);
        IBrgBarcodeBuilder BarcodeValue(string barcodeValue);
        IBrgBarcodeBuilder Brg(IBrgKey brgKey);
        IBrgBarcodeBuilder Satuan(string satuan);
        IBrgBarcodeBuilder Activate();
        IBrgBarcodeBuilder Deactivate();
    }

    public class BrgBarcodeBuilder : IBrgBarcodeBuilder
    {
        private BrgBarcodeModel _aggRoot = new BrgBarcodeModel();
        private readonly IBrgBarcodeDal _brgBarcodeDal;
        private readonly IBrgDal _brgDal;
        private readonly IBrgSatuanDal _brgSatuanDal;

        public BrgBarcodeBuilder(IBrgBarcodeDal brgBarcodeDal,
            IBrgDal brgDal,
            IBrgSatuanDal brgSatuanDal)
        {
            _brgBarcodeDal = brgBarcodeDal;
            _brgDal = brgDal;
            _brgSatuanDal = brgSatuanDal;
        }

        public BrgBarcodeModel Build()
        {
            _aggRoot.RemoveNull();
            return _aggRoot;
        }

        public IBrgBarcodeBuilder Create()
        {
            _aggRoot = new BrgBarcodeModel();
            return this;
        }

        public IBrgBarcodeBuilder Load(IBrgBarcodeKey brgBarcodeKey)
        {
            _aggRoot = _brgBarcodeDal.GetData(brgBarcodeKey)
                       ?? throw new KeyNotFoundException($"BrgBarcodeId not found ({brgBarcodeKey.BrgBarcodeId})");
            return this;
        }

        public IBrgBarcodeBuilder Attach(BrgBarcodeModel brgBarcode)
        {
            _aggRoot = brgBarcode;
            return this;
        }

        public IBrgBarcodeBuilder BarcodeValue(string barcodeValue)
        {
            _aggRoot.BarcodeValue = BrgBarcodeModel.NormalizeBarcodeValue(barcodeValue);
            return this;
        }

        public IBrgBarcodeBuilder Brg(IBrgKey brgKey)
        {
            var brg = _brgDal.GetData(brgKey)
                      ?? throw new KeyNotFoundException($"BrgId not found ({brgKey.BrgId})");
            _aggRoot.BrgId = brg.BrgId;
            _aggRoot.BrgCode = brg.BrgCode;
            _aggRoot.BrgName = brg.BrgName;
            return this;
        }

        public IBrgBarcodeBuilder Satuan(string satuan)
        {
            if (satuan.IsNullOrEmpty())
            {
                _aggRoot.Satuan = string.Empty;
                return this;
            }

            if (_aggRoot.BrgId.IsNullOrEmpty())
                throw new ArgumentException("Brg must be set before Satuan");

            var listSatuan = _brgSatuanDal.ListData(new BrgModel(_aggRoot.BrgId))?.ToList()
                             ?? new List<BrgSatuanModel>();
            if (!listSatuan.Any(x => x.Satuan == satuan))
                throw new ArgumentException($"Satuan not found for Brg ({satuan})");

            _aggRoot.Satuan = satuan;
            return this;
        }

        public IBrgBarcodeBuilder Activate()
        {
            _aggRoot.IsAktif = true;
            return this;
        }

        public IBrgBarcodeBuilder Deactivate()
        {
            _aggRoot.IsAktif = false;
            return this;
        }
    }
}

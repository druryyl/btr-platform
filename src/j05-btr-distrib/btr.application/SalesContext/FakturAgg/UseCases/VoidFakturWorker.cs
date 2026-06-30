using btr.application.FinanceContext.PiutangAgg.Workers;
using btr.application.InventoryContext.StokAgg.GenStokUseCase;
using btr.application.SalesContext.FakturAgg.Workers;
using btr.application.SalesContext.FakturControlAgg;
using btr.domain.FinanceContext.PiutangAgg;
using btr.domain.SalesContext.FakturAgg;
using btr.domain.SupportContext.UserAgg;
using btr.nuna.Application;
using System;

namespace btr.application.SalesContext.FakturAgg.UseCases
{
    public class VoidFakturRequest : IFakturKey, IUserKey
    {
        public VoidFakturRequest(string fakturId, string userId, int voidReasonCode, string voidReasonNote = "")
        {
            FakturId = fakturId;
            UserId = userId;
            VoidReasonCode = voidReasonCode;
            VoidReasonNote = voidReasonNote ?? string.Empty;
        }
        public string FakturId { get; set; }
        public string UserId { get; set; }
        public int VoidReasonCode { get; set; }
        public string VoidReasonNote { get; set; }
    }

    public interface IVoidFakturWorker : INunaServiceVoid<VoidFakturRequest>
    {
    }

    public class VoidFakturWorker : IVoidFakturWorker
    {
        private readonly IFakturBuilder _fakturBuilder;
        private readonly IRollBackStokWorker _rollBackStokWorker;
        private readonly IFakturControlBuilder _fakturControlBuilder;
        private readonly IFakturControlWriter _fakturControlWriter;
        private readonly IFakturWriter _fakturWriter;
        private readonly IPiutangWriter _piutangWriter;

        public VoidFakturWorker(IFakturBuilder fakturBuilder,
            IRollBackStokWorker rollBackStokWorker,
            IFakturControlBuilder fakturControlBuilder,
            IFakturControlWriter fakturControlWriter,
            IFakturWriter fakturWriter,
            IPiutangWriter piutangWriter)
        {
            _fakturBuilder = fakturBuilder;
            _rollBackStokWorker = rollBackStokWorker;
            _fakturControlBuilder = fakturControlBuilder;
            _fakturControlWriter = fakturControlWriter;
            _fakturWriter = fakturWriter;
            _piutangWriter = piutangWriter;
        }

        public void Execute(VoidFakturRequest req)
        {
            if (!FakturVoidReason.IsValid(req.VoidReasonCode))
                throw new ArgumentException("Alasan void wajib dipilih.");

            //   void faktur
            var faktur = _fakturBuilder
                .Load(req)
                .Void((IUserKey)req, req.VoidReasonCode, req.VoidReasonNote ?? string.Empty)
                .Build();
            
            //  unpost faktur control
            var fakturControl = _fakturControlBuilder
                .LoadOrCreate(req)
                .CancelPost(req)
                .Build();

            //  cancel piutang
            var piutangKey = new PiutangModel(faktur.FakturId);

            //  rollback stok
            var rollBackReq = new RollBackStokRequest(req.FakturId);

            //  apply database
            using (var trans = TransHelper.NewScope())
            {
                _piutangWriter.Delete(piutangKey);
                _rollBackStokWorker.Execute(rollBackReq);
                _ = _fakturWriter.Save(faktur);
                _fakturControlWriter.Save(fakturControl);
                trans.Complete();
            }
        }
    }
}

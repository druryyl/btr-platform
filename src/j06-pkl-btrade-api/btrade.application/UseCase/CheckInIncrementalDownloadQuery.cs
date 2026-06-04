using btrade.application.Contract;
using btrade.domain.CheckInFeature;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;
using Nuna.Lib.ValidationHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.UseCase
{
    public record CheckInIncrementalDownloadQuery(string Tgl1, string Tgl2, string ServerId)
        : IRequest<IEnumerable<CheckInType>>, IServerId;

    public class CheckInIncrementalDownloadQueryHandler : IRequestHandler<CheckInIncrementalDownloadQuery, IEnumerable<CheckInType>>
    {
        private readonly ICheckInDal _checkInDal;

        public CheckInIncrementalDownloadQueryHandler(ICheckInDal checkInDal)
        {
            _checkInDal = checkInDal;
        }

        public Task<IEnumerable<CheckInType>> Handle(CheckInIncrementalDownloadQuery request, CancellationToken cancellationToken)
        {
            var periode = new Periode(request.Tgl1.ToDate(DateFormatEnum.YMD), request.Tgl2.ToDate(DateFormatEnum.YMD));
            var checkIns = _checkInDal.ListData(periode, request)?.ToList() ?? new List<CheckInType>();
            if (checkIns.Count == 0)
                return Task.FromResult(Enumerable.Empty<CheckInType>());

            var checkInsToDownload = checkIns
                .Where(x => x.StatusSync == "TERKIRIM").ToList();

            using var trans = TransHelper.NewScope();
            foreach (var checkIn in checkInsToDownload)
            {
                var updatedCheckIn = checkIn with { StatusSync = "DOWNLOADED" };
                _checkInDal.Update(updatedCheckIn);
            }
            trans.Complete();

            return Task.FromResult(checkInsToDownload.AsEnumerable());
        }
    }
}

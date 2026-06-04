using btrade.application.Contract;
using btrade.domain.CheckInFeature;
using MediatR;
using Nuna.Lib.TransactionHelper;


namespace btrade.application.UseCase
{
    public record CheckInUploadCommand(
        string CheckInId,
        string CheckInDate,
        string CheckInTime,
        string UserEmail,
        float CheckInLatitude,
        float CheckInLongitude,
        float Accuracy,
        string CustomerId,
        string CustomerCode,
        string CustomerName,
        string CustomerAddress,
        double CustomerLatitude,
        double CustomerLongitude,
        string StatusSync,
        string ServerId) : IRequest;

    public class CheckInUploadCommandHandler : IRequestHandler<CheckInUploadCommand>
    {
        private readonly ICheckInDal _checkInDal;

        public CheckInUploadCommandHandler(ICheckInDal checkInDal)
        {
            _checkInDal = checkInDal;
        }

        public Task Handle(CheckInUploadCommand request, CancellationToken cancellationToken)
        {
            var model = new CheckInType(
                request.CheckInId,
                request.CheckInDate,
                request.CheckInTime,
                request.UserEmail,
                request.CheckInLatitude,
                request.CheckInLongitude,
                request.Accuracy,
                request.CustomerId,
                request.CustomerCode,
                request.CustomerName,
                request.CustomerAddress,
                request.CustomerLatitude,
                request.CustomerLongitude,
                "TERKIRIM",
                request.ServerId);

            using var trans = TransHelper.NewScope();

            _checkInDal.Delete(new CheckInKey(request.CheckInId));
            _checkInDal.Insert(model);

            trans.Complete();
            return Task.CompletedTask;
        }
    }

    // Helper class for the key interface
    public record CheckInKey(string CheckInId) : ICheckInKey;
}

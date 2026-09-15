using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record BarcodeRegistrationPendingQuery(string ServerId)
    : IRequest<IEnumerable<BarcodeRegistrationRequestType>>, IServerId;

public class BarcodeRegistrationPendingQueryHandler
    : IRequestHandler<BarcodeRegistrationPendingQuery, IEnumerable<BarcodeRegistrationRequestType>>
{
    private readonly IBarcodeRegistrationDal _barcodeRegistrationDal;

    public BarcodeRegistrationPendingQueryHandler(IBarcodeRegistrationDal barcodeRegistrationDal)
    {
        _barcodeRegistrationDal = barcodeRegistrationDal;
    }

    public Task<IEnumerable<BarcodeRegistrationRequestType>> Handle(
        BarcodeRegistrationPendingQuery request,
        CancellationToken cancellationToken)
    {
        var listData = _barcodeRegistrationDal.ListData(request);
        var result = listData.HasValue
            ? listData.Value.Where(x => x.Status == "PENDING")
            : Enumerable.Empty<BarcodeRegistrationRequestType>();
        return Task.FromResult(result);
    }
}

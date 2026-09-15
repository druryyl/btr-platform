using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record BarcodeRegistrationStatusQuery(string ServerId, string RequestedBy)
    : IRequest<IEnumerable<BarcodeRegistrationRequestType>>, IServerId;

public class BarcodeRegistrationStatusQueryHandler
    : IRequestHandler<BarcodeRegistrationStatusQuery, IEnumerable<BarcodeRegistrationRequestType>>
{
    private readonly IBarcodeRegistrationDal _barcodeRegistrationDal;

    public BarcodeRegistrationStatusQueryHandler(IBarcodeRegistrationDal barcodeRegistrationDal)
    {
        _barcodeRegistrationDal = barcodeRegistrationDal;
    }

    public Task<IEnumerable<BarcodeRegistrationRequestType>> Handle(
        BarcodeRegistrationStatusQuery request,
        CancellationToken cancellationToken)
    {
        //  §8.3 — the caller only learns the outcome of its own requests.
        var listData = _barcodeRegistrationDal.ListData(request);
        var result = listData.HasValue
            ? listData.Value.Where(x => x.RequestedBy == request.RequestedBy)
            : Enumerable.Empty<BarcodeRegistrationRequestType>();
        return Task.FromResult(result);
    }
}

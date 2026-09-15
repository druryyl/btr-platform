using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record BarcodeSyncListQuery(string ServerId) : IRequest<IEnumerable<BarcodeType>>, IServerId;

public class BarcodeSyncListQueryHandler : IRequestHandler<BarcodeSyncListQuery, IEnumerable<BarcodeType>>
{
    private readonly IBarcodeDal _barcodeDal;

    public BarcodeSyncListQueryHandler(IBarcodeDal barcodeDal)
    {
        _barcodeDal = barcodeDal;
    }

    public Task<IEnumerable<BarcodeType>> Handle(BarcodeSyncListQuery request, CancellationToken cancellationToken)
    {
        var listData = _barcodeDal.ListData(request);
        var result = listData.HasValue
            ? listData.Value
            : Enumerable.Empty<BarcodeType>();
        return Task.FromResult(result);
    }
}

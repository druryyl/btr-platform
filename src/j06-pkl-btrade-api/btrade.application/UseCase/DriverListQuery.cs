using btrade.application.Contract;
using btrade.domain.DriverFeature;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record DriverListDataQuery(string ServerId) : IRequest<IEnumerable<DriverType>>, IServerId;

public class DriverListDataHandler : IRequestHandler<DriverListDataQuery, IEnumerable<DriverType>>
{
    private readonly IDriverDal _driverDal;

    public DriverListDataHandler(IDriverDal driverDal)
    {
        _driverDal = driverDal;
    }

    public Task<IEnumerable<DriverType>> Handle(DriverListDataQuery request, CancellationToken cancellationToken)
    {
        var listData = _driverDal.ListData(request);
        return Task.FromResult(listData.HasValue
            ? listData.Value
            : Enumerable.Empty<DriverType>());
    }
}

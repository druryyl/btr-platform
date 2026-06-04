using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record SalesPersonListDataQuery(string ServerId) : IRequest<IEnumerable<SalesPersonType>>, IServerId;

public class SalesPersonListDataHandler : IRequestHandler<SalesPersonListDataQuery, IEnumerable<SalesPersonType>>
{
    private readonly ISalesPersonDal _salesPersonDal;

    public SalesPersonListDataHandler(ISalesPersonDal salesPersonDal)
    {
        _salesPersonDal = salesPersonDal;
    }

    public Task<IEnumerable<SalesPersonType>> Handle(SalesPersonListDataQuery request, CancellationToken cancellationToken)
    {
        var listData = _salesPersonDal.ListData(request);
        return Task.FromResult(listData.HasValue
            ? listData.Value
            : Enumerable.Empty<SalesPersonType>());
    }
}

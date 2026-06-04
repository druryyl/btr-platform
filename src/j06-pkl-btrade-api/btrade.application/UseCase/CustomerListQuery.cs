using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;

namespace btrade.application.UseCase;

public record CustomerListDataQuery(string ServerId) : IRequest<IEnumerable<CustomerType>>, IServerId;

public class CustomerListDataHandler : IRequestHandler<CustomerListDataQuery, IEnumerable<CustomerType>>
{
    private readonly ICustomerDal _customerDal;

    public CustomerListDataHandler(ICustomerDal customerDal)
    {
        _customerDal = customerDal;
    }

    public Task<IEnumerable<CustomerType>> Handle(CustomerListDataQuery request, CancellationToken cancellationToken)
    {
        var listData = _customerDal.ListData(request);
        return Task.FromResult(listData.HasValue
            ? listData.Value
            : Enumerable.Empty<CustomerType>());
    }
}

using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using Nuna.Lib.PatternHelper;

namespace btrade.application.UseCase;

public record IsValidSalesEmailQuery(string Email) : IRequest<IsValidSalesEmailResponse>;

public record IsValidSalesEmailResponse(string Email, int IsValid);

public class IsValidSalesEmailHandler : IRequestHandler<IsValidSalesEmailQuery, IsValidSalesEmailResponse>
{
    private readonly ISalesPersonDal _salesPersonDal;

    public IsValidSalesEmailHandler(ISalesPersonDal salesPersonDal)
    {
        _salesPersonDal = salesPersonDal;
    }

    public Task<IsValidSalesEmailResponse> Handle(IsValidSalesEmailQuery request, CancellationToken cancellationToken)
    {
        var result = _salesPersonDal.GetByEmail(request.Email);
        int isValid = result.HasValue ? 1 : 0;
        return Task.FromResult(new IsValidSalesEmailResponse(request.Email, isValid));
    }
}

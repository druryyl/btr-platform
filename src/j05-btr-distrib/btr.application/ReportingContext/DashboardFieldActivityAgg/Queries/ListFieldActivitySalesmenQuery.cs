using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Queries
{
    public class ListFieldActivitySalesmenQuery : IRequest<FieldActivitySalesmenResponse>
    {
    }

    public class FieldActivitySalesmenResponse
    {
        public IList<FieldActivitySalesmanItem> Items { get; set; } = new List<FieldActivitySalesmanItem>();
    }

    public class FieldActivitySalesmanItem
    {
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string SalesPersonCode { get; set; }
        public string Email { get; set; }
        public bool HasEmail { get; set; }
    }

    public class ListFieldActivitySalesmenQueryHandler
        : IRequestHandler<ListFieldActivitySalesmenQuery, FieldActivitySalesmenResponse>
    {
        private readonly ISalesPersonDal _salesPersonDal;

        public ListFieldActivitySalesmenQueryHandler(ISalesPersonDal salesPersonDal)
        {
            _salesPersonDal = salesPersonDal;
        }

        public Task<FieldActivitySalesmenResponse> Handle(
            ListFieldActivitySalesmenQuery request,
            CancellationToken cancellationToken)
        {
            var items = (_salesPersonDal.ListData() ?? Enumerable.Empty<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>())
                .OrderBy(x => x.SalesPersonName)
                .Select(x => new FieldActivitySalesmanItem
                {
                    SalesPersonId = x.SalesPersonId,
                    SalesPersonName = x.SalesPersonName,
                    SalesPersonCode = x.SalesPersonCode,
                    Email = x.Email ?? string.Empty,
                    HasEmail = !string.IsNullOrWhiteSpace(x.Email)
                })
                .ToList();

            return Task.FromResult(new FieldActivitySalesmenResponse { Items = items });
        }
    }
}

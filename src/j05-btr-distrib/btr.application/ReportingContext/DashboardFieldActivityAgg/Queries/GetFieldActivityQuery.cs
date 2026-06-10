using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using MediatR;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Queries
{
    public class GetFieldActivityQuery : IRequest<FieldActivityResponse>
    {
        public string SalesPersonId { get; set; }
        public DateTime VisitDate { get; set; }
    }

    public class GetFieldActivityQueryHandler : IRequestHandler<GetFieldActivityQuery, FieldActivityResponse>
    {
        private readonly FieldActivityComposer _composer;

        public GetFieldActivityQueryHandler(FieldActivityComposer composer)
        {
            _composer = composer;
        }

        public Task<FieldActivityResponse> Handle(
            GetFieldActivityQuery request,
            CancellationToken cancellationToken)
        {
            var result = _composer.Compose(request.SalesPersonId, request.VisitDate);
            return Task.FromResult(result);
        }
    }
}

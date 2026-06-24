using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class GetEntityAnalyticsTypesQuery : IRequest<EntityAnalyticsTypesResponse>
    {
    }

    public class EntityAnalyticsTypesResponse
    {
        public List<EntityAnalyticsTypeDto> Types { get; set; } = new List<EntityAnalyticsTypeDto>();
    }

    public class EntityAnalyticsTypeDto
    {
        public string EntityType { get; set; }

        public string DisplayName { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsAvailable { get; set; }

        public string ProfileRouteTemplate { get; set; }
    }

    public class GetEntityAnalyticsTypesHandler
        : IRequestHandler<GetEntityAnalyticsTypesQuery, EntityAnalyticsTypesResponse>
    {
        private readonly IEntityAnalyticsService _service;

        public GetEntityAnalyticsTypesHandler(IEntityAnalyticsService service)
        {
            _service = service;
        }

        public Task<EntityAnalyticsTypesResponse> Handle(
            GetEntityAnalyticsTypesQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new EntityAnalyticsTypesResponse
            {
                Types = new List<EntityAnalyticsTypeDto>(_service.GetEnabledTypes())
            });
        }
    }
}

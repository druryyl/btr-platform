using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class GetEntityAnalyticsSearchQuery : IRequest<EntityAnalyticsSearchResponse>
    {
        public string EntityType { get; set; }

        public string Q { get; set; }

        public int Top { get; set; } = 10;
    }

    public class EntityAnalyticsSearchResponse
    {
        public string EntityType { get; set; }

        public List<EntitySearchResultDto> Results { get; set; } = new List<EntitySearchResultDto>();
    }

    public class EntitySearchResultDto
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public string ProfileRoute { get; set; }
    }

    public class GetEntityAnalyticsSearchHandler
        : IRequestHandler<GetEntityAnalyticsSearchQuery, EntityAnalyticsSearchResponse>
    {
        private const int MinQueryLength = 2;
        private const int DefaultTop = 10;
        private const int MaxTop = 25;

        private readonly IEntityAnalyticsRepository _repository;
        private readonly IEntityTypeRegistry _entityTypes;

        public GetEntityAnalyticsSearchHandler(
            IEntityAnalyticsRepository repository,
            IEntityTypeRegistry entityTypes)
        {
            _repository = repository;
            _entityTypes = entityTypes;
        }

        public Task<EntityAnalyticsSearchResponse> Handle(
            GetEntityAnalyticsSearchQuery request,
            CancellationToken cancellationToken)
        {
            var normalizedType = _entityTypes.NormalizeEntityTypeCode(request.EntityType);
            if (normalizedType is null)
                throw new ArgumentException($"Unknown entity type: {request.EntityType}", nameof(request.EntityType));

            var query = request.Q?.Trim();
            if (string.IsNullOrWhiteSpace(query) || query.Length < MinQueryLength)
                throw new ArgumentException($"Search query must be at least {MinQueryLength} characters.", nameof(request.Q));

            var top = request.Top <= 0 ? DefaultTop : Math.Min(request.Top, MaxTop);
            var identities = _repository.SearchEntities(normalizedType, query, top);

            var results = identities
                .Select(identity => new EntitySearchResultDto
                {
                    EntityType = identity.EntityType,
                    EntityId = identity.EntityCode,
                    EntityCode = identity.EntityCode,
                    DisplayName = identity.DisplayName,
                    IsActive = identity.IsActive,
                    ProfileRoute = BuildProfileRoute(normalizedType, identity.EntityCode)
                })
                .ToList();

            return Task.FromResult(new EntityAnalyticsSearchResponse
            {
                EntityType = normalizedType,
                Results = results
            });
        }

        private string BuildProfileRoute(string entityType, string entityCode)
        {
            if (!_entityTypes.TryGet(entityType, out var registration)
                || string.IsNullOrWhiteSpace(registration.ProfileRouteTemplate))
            {
                return $"/analytics/{entityType}/{entityCode}";
            }

            return registration.ProfileRouteTemplate.Replace("{code}", entityCode);
        }
    }
}

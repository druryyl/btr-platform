using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class GetMapPresetsQuery : IRequest<MapPresetsResponse>
    {
        public string EntityType { get; set; }
    }

    public class MapPresetsResponse
    {
        public string EntityType { get; set; }

        public List<MapPresetDto> Presets { get; set; } = new List<MapPresetDto>();
    }

    public class GetMapPresetsHandler : IRequestHandler<GetMapPresetsQuery, MapPresetsResponse>
    {
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;

        public GetMapPresetsHandler(IKpiRegistry kpiRegistry, IEntityTypeRegistry entityTypes)
        {
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
        }

        public Task<MapPresetsResponse> Handle(GetMapPresetsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            var presets = EntityMapPresetRegistry.GetPresetsForEntityType(request.EntityType)
                .Select(p =>
                {
                    _kpiRegistry.TryGetMetadata(p.AxisXKpiId, out var axisX);
                    _kpiRegistry.TryGetMetadata(p.AxisYKpiId, out var axisY);
                    _kpiRegistry.TryGetMetadata(p.BubbleKpiId, out var bubble);
                    _kpiRegistry.TryGetMetadata(p.BubbleColorKpiId, out var bubbleColor);

                    return new MapPresetDto
                    {
                        PresetId = p.PresetId,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        AxisXKpiId = p.AxisXKpiId,
                        AxisYKpiId = p.AxisYKpiId,
                        AxisXLabel = axisX?.DisplayName ?? p.AxisXKpiId,
                        AxisYLabel = axisY?.DisplayName ?? p.AxisYKpiId,
                        BubbleKpiId = p.BubbleKpiId,
                        BubbleColorKpiId = p.BubbleColorKpiId,
                        BubbleLabel = bubble?.DisplayName ?? p.BubbleKpiId,
                        BubbleColorLabel = bubbleColor?.DisplayName ?? p.BubbleColorKpiId,
                        IsDefault = p.IsDefault
                    };
                })
                .ToList();

            return Task.FromResult(new MapPresetsResponse
            {
                EntityType = request.EntityType,
                Presets = presets
            });
        }
    }

    public class GetPopulationMapQuery : IRequest<PopulationMapResponseDto>
    {
        public string EntityType { get; set; }

        public string PresetId { get; set; }

        public string DimensionFilter { get; set; }

        public bool? AttentionOnly { get; set; }
    }

    public class GetPopulationMapHandler : IRequestHandler<GetPopulationMapQuery, PopulationMapResponseDto>
    {
        private readonly IEntityPopulationMapEngine _engine;

        public GetPopulationMapHandler(IEntityPopulationMapEngine engine)
        {
            _engine = engine;
        }

        public Task<PopulationMapResponseDto> Handle(GetPopulationMapQuery request, CancellationToken cancellationToken)
        {
            var result = _engine.BuildPopulationMap(new PopulationMapRequest
            {
                EntityType = request.EntityType,
                PresetId = request.PresetId,
                DimensionFilter = request.DimensionFilter,
                AttentionOnly = request.AttentionOnly
            });

            return Task.FromResult(result);
        }
    }

    public class GetPeerDistributionQuery : IRequest<PeerDistributionResponseDto>
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string KpiId { get; set; }

        public string DimensionFilter { get; set; }

        public string PeerGroupRuleId { get; set; }
    }

    public class GetPeerDistributionHandler : IRequestHandler<GetPeerDistributionQuery, PeerDistributionResponseDto>
    {
        private readonly IEntityPeerDistributionEngine _engine;

        public GetPeerDistributionHandler(IEntityPeerDistributionEngine engine)
        {
            _engine = engine;
        }

        public Task<PeerDistributionResponseDto> Handle(
            GetPeerDistributionQuery request,
            CancellationToken cancellationToken)
        {
            var result = _engine.BuildDistribution(new PeerDistributionRequest
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                KpiId = request.KpiId,
                DimensionFilter = request.DimensionFilter,
                PeerGroupRuleId = request.PeerGroupRuleId
            });

            return Task.FromResult(result);
        }
    }

    public class GetPeerGroupRulesQuery : IRequest<PeerGroupRulesResponseDto>
    {
        public string EntityType { get; set; }
    }

    public class GetPeerGroupRulesHandler : IRequestHandler<GetPeerGroupRulesQuery, PeerGroupRulesResponseDto>
    {
        private readonly IEntityTypeRegistry _entityTypes;

        public GetPeerGroupRulesHandler(IEntityTypeRegistry entityTypes)
        {
            _entityTypes = entityTypes;
        }

        public Task<PeerGroupRulesResponseDto> Handle(
            GetPeerGroupRulesQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            var rules = PeerGroupRuleCatalog.GetRulesForEntityType(request.EntityType);
            var defaultRuleId = PeerGroupRuleCatalog.GetDefaultRuleId(request.EntityType, _entityTypes);

            return Task.FromResult(new PeerGroupRulesResponseDto
            {
                EntityType = request.EntityType,
                DefaultRuleId = defaultRuleId,
                Rules = rules.Select(r => new PeerGroupRuleDto
                {
                    RuleId = r.RuleId,
                    DisplayLabel = r.DisplayLabel,
                    DimensionLabel = r.DimensionLabel,
                    IsDefault = string.Equals(r.RuleId, defaultRuleId, StringComparison.OrdinalIgnoreCase)
                        || (string.IsNullOrWhiteSpace(defaultRuleId) && r.IsDefault)
                }).ToList()
            });
        }
    }

    public class GetInvestigationLensesQuery : IRequest<InvestigationLensesResponse>
    {
        public string EntityType { get; set; }
    }

    public class InvestigationLensesResponse
    {
        public string EntityType { get; set; }

        public string DefaultLensId { get; set; }

        public List<InvestigationLensDto> Lenses { get; set; } = new List<InvestigationLensDto>();
    }

    public class InvestigationLensDto
    {
        public string LensId { get; set; }

        public string DisplayName { get; set; }

        public bool IsDefault { get; set; }

        public string DefaultPresetId { get; set; }

        public List<string> KpiIds { get; set; } = new List<string>();

        public List<string> AttentionCategories { get; set; } = new List<string>();

        public List<string> RelationshipDrivers { get; set; } = new List<string>();

        public List<string> EvidenceRoutes { get; set; } = new List<string>();
    }

    public class GetInvestigationLensesHandler
        : IRequestHandler<GetInvestigationLensesQuery, InvestigationLensesResponse>
    {
        private readonly IEntityTypeRegistry _entityTypes;

        public GetInvestigationLensesHandler(IEntityTypeRegistry entityTypes)
        {
            _entityTypes = entityTypes;
        }

        public Task<InvestigationLensesResponse> Handle(
            GetInvestigationLensesQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            var defaultLens = EntityInvestigationLensRegistry.ResolveDefaultLens(request.EntityType);

            return Task.FromResult(new InvestigationLensesResponse
            {
                EntityType = request.EntityType,
                DefaultLensId = defaultLens?.LensId,
                Lenses = EntityInvestigationLensRegistry.GetLensesForEntityType(request.EntityType)
                    .Select(l => new InvestigationLensDto
                    {
                        LensId = l.LensId,
                        DisplayName = l.DisplayName,
                        IsDefault = l.IsDefault,
                        DefaultPresetId = l.DefaultPresetId,
                        KpiIds = l.KpiIds?.ToList() ?? new List<string>(),
                        AttentionCategories = l.AttentionCategories?.ToList() ?? new List<string>(),
                        RelationshipDrivers = l.RelationshipDrivers?.ToList() ?? new List<string>(),
                        EvidenceRoutes = l.EvidenceRoutes?.ToList() ?? new List<string>()
                    })
                    .ToList()
            });
        }
    }
}

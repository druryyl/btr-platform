using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
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

                    return new MapPresetDto
                    {
                        PresetId = p.PresetId,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        AxisXKpiId = p.AxisXKpiId,
                        AxisYKpiId = p.AxisYKpiId,
                        AxisXLabel = axisX?.DisplayName ?? p.AxisXKpiId,
                        AxisYLabel = axisY?.DisplayName ?? p.AxisYKpiId,
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
                DimensionFilter = request.DimensionFilter
            });

            return Task.FromResult(result);
        }
    }
}

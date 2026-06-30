using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.EntityAnalytics
{
    [Authorize]
    [RoutePrefix("api/entity-analytics")]
    public class EntityAnalyticsController : ApiController
    {
        private readonly IMediator _mediator;

        public EntityAnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("types")]
        public async Task<IHttpActionResult> GetTypes()
        {
            var result = await _mediator.Send(new GetEntityAnalyticsTypesQuery());
            return Ok(ApiResponse<EntityAnalyticsTypesResponse>.Success(result));
        }

        [HttpGet, Route("search")]
        public async Task<IHttpActionResult> Search(
            [FromUri] string entityType = null,
            [FromUri] string q = null,
            [FromUri] int top = 10)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityAnalyticsSearchResponse>.Error(400, "EntityType is required."));
            }

            try
            {
                var result = await _mediator.Send(new GetEntityAnalyticsSearchQuery
                {
                    EntityType = entityType,
                    Q = q,
                    Top = top
                });
                return Ok(ApiResponse<EntityAnalyticsSearchResponse>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityAnalyticsSearchResponse>.Error(400, ex.Message));
            }
        }

        [HttpGet, Route("compare")]
        public async Task<IHttpActionResult> Compare(
            [FromUri] string entityType = null,
            [FromUri] string entityIds = null,
            [FromUri] string kpiIds = null,
            [FromUri] int? periodYear = null,
            [FromUri] int? periodMonth = null)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityIds))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityCompareResponse>.Error(400, "EntityType and EntityIds are required."));
            }

            try
            {
                var result = await _mediator.Send(new CompareEntitiesQuery
                {
                    EntityType = entityType,
                    EntityIds = entityIds,
                    KpiIds = kpiIds,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                });
                return Ok(ApiResponse<EntityCompareResponse>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityCompareResponse>.Error(400, ex.Message));
            }
        }

        [HttpGet, Route("presets")]
        public async Task<IHttpActionResult> GetPresets([FromUri] string entityType = null)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<MapPresetsResponse>.Error(400, "EntityType is required."));
            }

            try
            {
                var result = await _mediator.Send(new GetMapPresetsQuery { EntityType = entityType });
                return Ok(ApiResponse<MapPresetsResponse>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<MapPresetsResponse>.Error(400, ex.Message));
            }
        }

        [HttpGet, Route("population")]
        public async Task<IHttpActionResult> GetPopulation(
            [FromUri] string entityType = null,
            [FromUri] string presetId = null,
            [FromUri] string dimensionFilter = null,
            [FromUri] bool? attentionOnly = null)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<PopulationMapResponseDto>.Error(400, "EntityType is required."));
            }

            try
            {
                var result = await _mediator.Send(new GetPopulationMapQuery
                {
                    EntityType = entityType,
                    PresetId = presetId,
                    DimensionFilter = dimensionFilter,
                    AttentionOnly = attentionOnly
                });
                return Ok(ApiResponse<PopulationMapResponseDto>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<PopulationMapResponseDto>.Error(400, ex.Message));
            }
        }

        [HttpGet, Route("peer-distribution")]
        public async Task<IHttpActionResult> GetPeerDistribution(
            [FromUri] string entityType = null,
            [FromUri] string entityId = null,
            [FromUri] string kpiId = null,
            [FromUri] string dimensionFilter = null)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(kpiId))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<PeerDistributionResponseDto>.Error(400, "EntityType, EntityId, and KpiId are required."));
            }

            try
            {
                var result = await _mediator.Send(new GetPeerDistributionQuery
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    KpiId = kpiId,
                    DimensionFilter = dimensionFilter
                });
                return Ok(ApiResponse<PeerDistributionResponseDto>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<PeerDistributionResponseDto>.Error(400, ex.Message));
            }
        }

        [HttpGet, Route("{entityType}/{entityId}")]
        public async Task<IHttpActionResult> GetProfile(string entityType, string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityPerformanceProfileResponse>.Error(400, "EntityType and EntityId are required."));
            }

            try
            {
                var result = await _mediator.Send(new GetEntityPerformanceProfileQuery
                {
                    EntityType = entityType,
                    EntityId = entityId
                });
                return Ok(ApiResponse<EntityPerformanceProfileResponse>.Success(result));
            }
            catch (System.ArgumentException ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<EntityPerformanceProfileResponse>.Error(400, ex.Message));
            }
        }
    }
}

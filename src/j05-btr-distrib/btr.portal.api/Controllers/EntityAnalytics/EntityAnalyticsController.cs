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

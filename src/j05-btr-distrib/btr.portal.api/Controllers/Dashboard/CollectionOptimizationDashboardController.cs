using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/collection-optimization")]
    public class CollectionOptimizationDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CollectionOptimizationDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCollectionOptimizationQuery());
            return Ok(ApiResponse<DashboardCollectionOptimizationResponse>.Success(result));
        }
    }
}

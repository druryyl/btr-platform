using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/inventory-optimization")]
    public class InventoryOptimizationDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public InventoryOptimizationDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardInventoryOptimizationQuery());
            return Ok(ApiResponse<DashboardInventoryOptimizationResponse>.Success(result));
        }
    }
}

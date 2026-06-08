using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/inventory-risk")]
    public class InventoryRiskDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public InventoryRiskDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardInventoryRiskQuery());
            return Ok(ApiResponse<DashboardInventoryRiskResponse>.Success(result));
        }
    }
}

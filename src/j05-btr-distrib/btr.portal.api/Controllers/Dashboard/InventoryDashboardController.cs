using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/inventory")]
    public class InventoryDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public InventoryDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardInventoryQuery());
            return Ok(ApiResponse<DashboardInventoryResponse>.Success(result));
        }
    }
}

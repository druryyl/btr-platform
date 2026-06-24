using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardInventoryForecastAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/inventory-forecast")]
    public class InventoryForecastDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public InventoryForecastDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardInventoryForecastQuery());
            return Ok(ApiResponse<DashboardInventoryForecastResponse>.Success(result));
        }
    }
}

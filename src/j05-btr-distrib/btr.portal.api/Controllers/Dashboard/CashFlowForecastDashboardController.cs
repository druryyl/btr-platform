using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/cash-flow-forecast")]
    public class CashFlowForecastDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CashFlowForecastDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCashFlowForecastQuery());
            return Ok(ApiResponse<DashboardCashFlowForecastResponse>.Success(result));
        }
    }
}

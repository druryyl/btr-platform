using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/customer-risk-forecast")]
    public class CustomerRiskForecastDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CustomerRiskForecastDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCustomerRiskForecastQuery());
            return Ok(ApiResponse<DashboardCustomerRiskForecastResponse>.Success(result));
        }
    }
}

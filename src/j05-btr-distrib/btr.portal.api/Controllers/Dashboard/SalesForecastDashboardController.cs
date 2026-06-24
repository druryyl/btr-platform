using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/sales-forecast")]
    public class SalesForecastDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public SalesForecastDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardSalesForecastQuery());
            return Ok(ApiResponse<DashboardSalesForecastResponse>.Success(result));
        }
    }
}

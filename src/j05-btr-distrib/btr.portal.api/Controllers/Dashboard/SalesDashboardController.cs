using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/sales")]
    public class SalesDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public SalesDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardSalesQuery());
            return Ok(ApiResponse<DashboardSalesResponse>.Success(result));
        }
    }
}

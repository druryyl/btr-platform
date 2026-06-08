using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSalesmanAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/salesmen")]
    public class SalesmanDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public SalesmanDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardSalesmanQuery());
            return Ok(ApiResponse<DashboardSalesmanResponse>.Success(result));
        }
    }
}

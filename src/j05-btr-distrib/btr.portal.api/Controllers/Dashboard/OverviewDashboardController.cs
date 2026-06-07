using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardOverviewAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/overview")]
    public class OverviewDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public OverviewDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardOverviewQuery());
            return Ok(ApiResponse<DashboardOverviewResponse>.Success(result));
        }
    }
}

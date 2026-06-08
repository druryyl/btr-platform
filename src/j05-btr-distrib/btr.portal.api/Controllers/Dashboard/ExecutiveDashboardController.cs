using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardExecutiveAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/executive")]
    public class ExecutiveDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public ExecutiveDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardExecutiveQuery());
            return Ok(ApiResponse<DashboardExecutiveResponse>.Success(result));
        }
    }
}

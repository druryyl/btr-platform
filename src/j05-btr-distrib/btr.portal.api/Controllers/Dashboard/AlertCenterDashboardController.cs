using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/alerts")]
    public class AlertCenterDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public AlertCenterDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardAlertCenterQuery());
            return Ok(ApiResponse<DashboardAlertCenterResponse>.Success(result));
        }
    }
}

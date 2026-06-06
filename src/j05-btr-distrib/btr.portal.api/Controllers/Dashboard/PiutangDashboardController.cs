using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/piutang")]
    public class PiutangDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public PiutangDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardPiutangQuery());
            return Ok(ApiResponse<DashboardPiutangResponse>.Success(result));
        }
    }
}

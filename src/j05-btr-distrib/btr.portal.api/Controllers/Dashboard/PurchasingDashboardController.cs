using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/purchasing")]
    public class PurchasingDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public PurchasingDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardPurchasingQuery());
            return Ok(ApiResponse<DashboardPurchasingResponse>.Success(result));
        }
    }
}

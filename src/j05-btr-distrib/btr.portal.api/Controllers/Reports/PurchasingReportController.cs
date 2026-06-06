using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.PurchasingReportAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Reports
{
    [Authorize]
    [RoutePrefix("api/reports/purchasing")]
    public class PurchasingReportController : ApiController
    {
        private readonly IMediator _mediator;

        public PurchasingReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetPurchasingReportQuery());
            return Ok(ApiResponse<PurchasingReportResponse>.Success(result));
        }
    }
}

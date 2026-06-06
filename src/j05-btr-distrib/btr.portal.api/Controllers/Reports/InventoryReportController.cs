using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.InventoryReportAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Reports
{
    [Authorize]
    [RoutePrefix("api/reports/inventory")]
    public class InventoryReportController : ApiController
    {
        private readonly IMediator _mediator;

        public InventoryReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetInventoryReportQuery());
            return Ok(ApiResponse<InventoryReportResponse>.Success(result));
        }
    }
}

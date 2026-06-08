using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.SalesReportAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Reports
{
    [Authorize]
    [RoutePrefix("api/reports/sales")]
    public class SalesReportController : ApiController
    {
        private readonly IMediator _mediator;

        public SalesReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get(
            [FromUri] System.DateTime? from = null,
            [FromUri] System.DateTime? to = null)
        {
            var result = await _mediator.Send(new GetSalesReportQuery
            {
                From = from,
                To = to,
            });
            return Ok(ApiResponse<SalesReportResponse>.Success(result));
        }
    }
}

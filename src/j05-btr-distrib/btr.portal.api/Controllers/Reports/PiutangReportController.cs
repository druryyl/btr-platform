using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.PiutangReportAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Reports
{
    [Authorize]
    [RoutePrefix("api/reports/piutang")]
    public class PiutangReportController : ApiController
    {
        private readonly IMediator _mediator;

        public PiutangReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get(
            [FromUri] System.DateTime? from = null,
            [FromUri] System.DateTime? to = null,
            [FromUri] string dateField = "DueDate")
        {
            var result = await _mediator.Send(new GetPiutangReportQuery
            {
                From = from,
                To = to,
                DateField = dateField,
            });
            return Ok(ApiResponse<PiutangReportResponse>.Success(result));
        }
    }
}

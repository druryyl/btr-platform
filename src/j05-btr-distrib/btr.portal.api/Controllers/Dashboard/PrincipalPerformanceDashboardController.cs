using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/principal-performance")]
    public class PrincipalPerformanceDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public PrincipalPerformanceDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetPrincipalPerformanceQuery());
            return Ok(ApiResponse<PrincipalPerformanceResponse>.Success(result));
        }

        [HttpGet, Route("evidence")]
        public async Task<IHttpActionResult> GetEvidence([FromUri] string supplierId = null)
        {
            var result = await _mediator.Send(new GetPrincipalSalesOutEvidenceQuery
            {
                SupplierId = supplierId
            });
            return Ok(ApiResponse<PrincipalSalesOutEvidenceResponse>.Success(result));
        }

        [HttpGet, Route("return-evidence")]
        public async Task<IHttpActionResult> GetReturnEvidence([FromUri] string supplierId = null)
        {
            var result = await _mediator.Send(new GetPrincipalReturnEvidenceQuery
            {
                SupplierId = supplierId
            });
            return Ok(ApiResponse<PrincipalReturnEvidenceResponse>.Success(result));
        }
    }
}

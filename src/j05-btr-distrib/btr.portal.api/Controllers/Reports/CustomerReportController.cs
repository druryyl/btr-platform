using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.CustomerReportAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Reports
{
    [Authorize]
    [RoutePrefix("api/reports/customers")]
    public class CustomerReportController : ApiController
    {
        private readonly IMediator _mediator;

        public CustomerReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get([FromUri] string customerCode = null)
        {
            var result = await _mediator.Send(new GetCustomerReportQuery
            {
                CustomerCode = customerCode
            });
            return Ok(ApiResponse<CustomerReportResponse>.Success(result));
        }
    }
}

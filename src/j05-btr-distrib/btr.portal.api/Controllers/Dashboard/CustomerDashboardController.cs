using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCustomerAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/customers")]
    public class CustomerDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CustomerDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCustomerQuery());
            return Ok(ApiResponse<DashboardCustomerResponse>.Success(result));
        }
    }
}

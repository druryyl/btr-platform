using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/customer-portfolio")]
    public class CustomerPortfolioDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CustomerPortfolioDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCustomerPortfolioQuery());
            return Ok(ApiResponse<DashboardCustomerPortfolioResponse>.Success(result));
        }
    }
}

using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSalesmanAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/salesmen")]
    public class SalesmanDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public SalesmanDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardSalesmanQuery());
            return Ok(ApiResponse<DashboardSalesmanResponse>.Success(result));
        }

        [HttpGet, Route("{salesPersonId}/principals")]
        public async Task<IHttpActionResult> GetPrincipals(string salesPersonId)
        {
            var result = await _mediator.Send(new GetSalesmanPrincipalAchievementQuery
            {
                SalesPersonId = salesPersonId
            });
            return Ok(ApiResponse<SalesmanPrincipalAchievementResponse>.Success(result));
        }

        [HttpGet, Route("{salesPersonId}/trend")]
        public async Task<IHttpActionResult> GetTrend(string salesPersonId, int months = 12)
        {
            var result = await _mediator.Send(new GetSalesmanAchievementTrendQuery
            {
                SalesPersonId = salesPersonId,
                Months = months
            });
            return Ok(ApiResponse<SalesmanAchievementTrendResponse>.Success(result));
        }
    }
}

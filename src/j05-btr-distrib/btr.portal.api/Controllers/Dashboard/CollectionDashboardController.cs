using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardCollectionAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/collection")]
    public class CollectionDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public CollectionDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var result = await _mediator.Send(new GetDashboardCollectionQuery());
            return Ok(ApiResponse<DashboardCollectionResponse>.Success(result));
        }
    }
}

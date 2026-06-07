using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardSnapshotAgg.Commands;
using btr.portal.api.Models;
using MediatR;
using NLog;

namespace btr.portal.api.Controllers.Admin
{
    [Authorize]
    [RoutePrefix("api/admin/dashboard")]
    public class AdminDashboardRefreshController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMediator _mediator;

        public AdminDashboardRefreshController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost, Route("refresh")]
        public async Task<IHttpActionResult> Refresh(DashboardRefreshRequest request)
        {
            var domain = request?.Domain ?? "All";
            Logger.Info("Manual dashboard snapshot refresh requested for domain {Domain}", domain);

            var result = await _mediator.Send(new RefreshDashboardSnapshotsCommand
            {
                Domain = domain
            });

            return Ok(ApiResponse<RefreshDashboardSnapshotsResponse>.Success(result));
        }
    }
}

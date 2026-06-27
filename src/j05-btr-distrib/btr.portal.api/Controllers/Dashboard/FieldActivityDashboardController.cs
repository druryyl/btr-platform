using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Queries;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Queries;
using btr.portal.api.Models;
using MediatR;

namespace btr.portal.api.Controllers.Dashboard
{
    [Authorize]
    [RoutePrefix("api/dashboard/field-activity")]
    public class FieldActivityDashboardController : ApiController
    {
        private readonly IMediator _mediator;

        public FieldActivityDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get(string salesPersonId, DateTime? visitDate)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Error(400, "salesPersonId is required."));
            }

            if (!visitDate.HasValue)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Error(400, "visitDate is required."));
            }

            var result = await _mediator.Send(new GetFieldActivityQuery
            {
                SalesPersonId = salesPersonId,
                VisitDate = visitDate.Value
            });

            return Ok(ApiResponse<btr.application.ReportingContext.DashboardFieldActivityAgg.Models.FieldActivityResponse>.Success(result));
        }

        [HttpGet, Route("salesmen")]
        public async Task<IHttpActionResult> ListSalesmen()
        {
            var result = await _mediator.Send(new ListFieldActivitySalesmenQuery());
            return Ok(ApiResponse<FieldActivitySalesmenResponse>.Success(result));
        }

        [HttpGet, Route("overview")]
        public async Task<IHttpActionResult> GetOverview(DateTime? visitDate)
        {
            var result = await _mediator.Send(new GetFieldActivityOverviewQuery
            {
                VisitDate = visitDate
            });

            return Ok(ApiResponse<FieldActivityOverviewResponse>.Success(result));
        }
    }
}

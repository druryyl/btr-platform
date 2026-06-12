using System.Web.Http;
using btr.application.Portal;
using btr.portal.api.Models;

namespace btr.portal.api.Controllers
{
    [RoutePrefix("api/config")]
    [Authorize]
    public class PresentationConfigController : ApiController
    {
        private readonly IPresentationModeService _presentationModeService;
        private readonly IBusinessDateProvider _businessDateProvider;

        public PresentationConfigController(
            IPresentationModeService presentationModeService,
            IBusinessDateProvider businessDateProvider)
        {
            _presentationModeService = presentationModeService;
            _businessDateProvider = businessDateProvider;
        }

        [HttpGet, Route("presentation")]
        public IHttpActionResult GetPresentation()
        {
            var today = _businessDateProvider.Today;
            var data = new PresentationConfigResponse
            {
                Enabled = _presentationModeService.IsEnabled,
                BusinessDate = today.ToString("yyyy-MM-dd")
            };

            return Ok(ApiResponse<PresentationConfigResponse>.Success(data));
        }
    }
}

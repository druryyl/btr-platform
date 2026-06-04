using btrade.application.UseCase;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CheckInController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> UploadData(CheckInUploadCommand cmd)
        {
            await _mediator.Send(cmd);
            return Ok(new JSendOk("Done"));
        }

        [HttpGet]
        [Route("incremental/{tgl1}/{tgl2}/{serverId}")]
        public async Task<IActionResult> IncrementalDownload(string tgl1, string tgl2, string serverId)
        {
            var query = new CheckInIncrementalDownloadQuery(tgl1, tgl2, serverId);
            var result = await _mediator.Send(query);
            return Ok(new JSendOk(result));
        }
    }
}

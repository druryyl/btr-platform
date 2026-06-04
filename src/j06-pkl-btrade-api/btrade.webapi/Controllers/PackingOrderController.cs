using btrade.application.WarehouseFreature;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackingOrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PackingOrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{startTimestamp}/{warehouseCode}/{pageSize}")]
        public async Task<IActionResult> Download(string startTimestamp, string warehouseCode, int pageSize)
        {
            var query = new WrhDownloadPackingOrderCmd(startTimestamp, warehouseCode, pageSize);
            var response = await _mediator.Send(query);
            return Ok(new JSendOk(response));
        }

        [HttpPost]
        public async Task<IActionResult> Upload(WrhSavePackingOrderCmd cmd)
        {
            await _mediator.Send(cmd);
            return Ok(new JSendOk("Done"));
        }
        [HttpPost]
        [Route("bulk")]
        public async Task<IActionResult> BulkUpload(WrhBulkUploadPackingOrderCmd cmd)
        {
            await _mediator.Send(cmd);
            return Ok(new JSendOk("Done"));
        }
    }
}

using btrade.application.UseCase;
using btrade.domain.BarcodeFeature;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BarcodeController : ControllerBase
{
    private readonly IMediator _mediator;

    public BarcodeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("sync")]
    public async Task<IActionResult> SyncData(BarcodeSyncRequest request)
    {
        var cmd = new BarcodeSyncCommand(
            request.ListUpsert ?? Enumerable.Empty<BarcodeType>(),
            request.ListRemove ?? Enumerable.Empty<string>(),
            User.GetServerId());
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }

    [HttpGet]
    [Route("~/api/barcodes/sync")]
    public async Task<IActionResult> ListData()
    {
        var query = new BarcodeSyncListQuery(User.GetServerId());
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }
}

public record BarcodeSyncRequest(
    IEnumerable<BarcodeType> ListUpsert,
    IEnumerable<string> ListRemove);

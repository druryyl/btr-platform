using btrade.application.UseCase;
using btrade.domain.BarcodeFeature;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BarcodeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SessionContextResolver _sessionContextResolver;

    public BarcodeController(
        IMediator mediator,
        SessionContextResolver sessionContextResolver)
    {
        _mediator = mediator;
        _sessionContextResolver = sessionContextResolver;
    }

    [HttpPost]
    [Route("sync")]
    public async Task<IActionResult> SyncData(BarcodeSyncRequest request)
    {
        //  TD-06 — [Authorize] is removed instance-wide, but the j07 publish
        //  path keeps its existing claim-based tenant for token-bearing
        //  consumers (unchanged; removing the attribute does not strip claims
        //  from a supplied token).
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
        //  TD-03/TD-04 — BGud's tenant comes from X-Session-Location through
        //  the single resolver; no JWT claims are read on this route. A
        //  missing/unmapped location surfaces as HTTP 400 (§9) via
        //  ErrorHandlerMiddleware.
        var serverId = _sessionContextResolver.ResolveTenant(
            Request.GetSessionLocation());
        var query = new BarcodeSyncListQuery(serverId);
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }
}

public record BarcodeSyncRequest(
    IEnumerable<BarcodeType> ListUpsert,
    IEnumerable<string> ListRemove);

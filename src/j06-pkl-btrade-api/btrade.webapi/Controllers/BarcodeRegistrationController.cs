using btrade.application.UseCase;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BarcodeRegistrationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SessionContextResolver _sessionContextResolver;

    public BarcodeRegistrationController(
        IMediator mediator,
        SessionContextResolver sessionContextResolver)
    {
        _mediator = mediator;
        _sessionContextResolver = sessionContextResolver;
    }

    [HttpGet]
    [Route("pending")]
    public async Task<IActionResult> Pending()
    {
        //  TD-06 — anonymous instance-wide, but pending/ack keep their existing
        //  claim-based behavior for token-bearing consumers (j07-btrade-sync).
        var query = new BarcodeRegistrationPendingQuery(User.GetServerId());
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }

    [HttpPost]
    [Route("ack")]
    public async Task<IActionResult> Ack(BarcodeRegistrationAckRequest request)
    {
        var cmd = new BarcodeRegistrationAckCommand(
            request.BarcodeRegistrationId,
            request.Status,
            request.ProcessedNote,
            User.GetServerId());
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }

    [HttpPost]
    [Route("~/api/barcode-registration")]
    public async Task<IActionResult> Submit(BarcodeRegistrationSubmitRequest request)
    {
        //  TD-03/TD-04/TD-05 — tenant from X-Session-Location, actor from
        //  X-Session-Actor, both through the single resolver. The resolved BTR
        //  UserId is recorded verbatim as RequestedBy (identifier shape
        //  unchanged); no JWT claims are read on BGud's path.
        var serverId = _sessionContextResolver.ResolveTenant(
            Request.GetSessionLocation());
        var requestedBy = _sessionContextResolver.ResolveAccount(
            Request.GetSessionActor()).UserId;

        var cmd = new BarcodeRegistrationSubmitCommand(
            request.ClientRequestId,
            request.BarcodeValue,
            request.BrgId,
            request.Satuan,
            requestedBy,
            serverId);
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }

    [HttpGet]
    [Route("status")]
    public async Task<IActionResult> Status()
    {
        //  TD-05 — the caller still learns only the outcome of its own
        //  requests: the resolved actor UserId is the RequestedBy filter.
        var serverId = _sessionContextResolver.ResolveTenant(
            Request.GetSessionLocation());
        var requestedBy = _sessionContextResolver.ResolveAccount(
            Request.GetSessionActor()).UserId;

        var query = new BarcodeRegistrationStatusQuery(serverId, requestedBy);
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }
}

public record BarcodeRegistrationAckRequest(
    string BarcodeRegistrationId,
    string Status,
    string ProcessedNote);

public record BarcodeRegistrationSubmitRequest(
    string ClientRequestId,
    string BarcodeValue,
    string BrgId,
    string Satuan);

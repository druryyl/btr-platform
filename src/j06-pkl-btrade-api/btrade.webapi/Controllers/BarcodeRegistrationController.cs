using btrade.application.UseCase;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BarcodeRegistrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public BarcodeRegistrationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("pending")]
    public async Task<IActionResult> Pending()
    {
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
        var cmd = new BarcodeRegistrationSubmitCommand(
            request.ClientRequestId,
            request.BarcodeValue,
            request.BrgId,
            request.Satuan,
            User.GetUserId(),
            User.GetServerId());
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }

    [HttpGet]
    [Route("status")]
    public async Task<IActionResult> Status()
    {
        var query = new BarcodeRegistrationStatusQuery(User.GetServerId(), User.GetUserId());
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

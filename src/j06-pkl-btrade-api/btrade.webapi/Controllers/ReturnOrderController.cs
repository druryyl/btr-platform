using System.Text.Json;
using btrade.application.UseCase;
using btrade.domain.ReturnOrderFeature;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReturnOrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SessionContextResolver _sessionContextResolver;

    public ReturnOrderController(
        IMediator mediator,
        SessionContextResolver sessionContextResolver)
    {
        _mediator = mediator;
        _sessionContextResolver = sessionContextResolver;
    }

    [HttpPost]
    [Route("~/api/return-order")]
    public async Task<IActionResult> Submit(ReturnOrderSubmitRequest request)
    {
        //  TD-03/TD-04/TD-05/TD-06 — [Authorize] is removed instance-wide and
        //  the context comes from the session headers through the single
        //  resolver: X-Session-Location → ServerId, X-Session-Actor → the
        //  resolved BTR UserId recorded as SubmittedBy. No JWT claim is read
        //  on BGud's path.
        var serverId = _sessionContextResolver.ResolveTenant(
            Request.GetSessionLocation());
        var submittedBy = _sessionContextResolver.ResolveAccount(
            Request.GetSessionActor()).UserId;

        var cmd = new ReturnOrderUploadCommand(
            request.ReturnOrderId,
            request.ReturnOrderDate,
            request.WarehouseCode,
            request.CustomerId,
            request.CustomerName,
            request.SalesPersonId,
            request.SalesPersonName,
            request.DriverId,
            request.DriverName,
            request.Note,
            serverId,
            submittedBy,
            request.ListItem);
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }

    [HttpGet]
    [Route("incremental/{tgl1}/{tgl2}/{serverId}")]
    public async Task<IActionResult> IncrementalDownload(string tgl1, string tgl2, string serverId)
    {
        var query = new ReturnOrderIncrementalDownloadQuery(tgl1, tgl2, serverId);
        var result = await _mediator.Send(query);

        //  TD-15 — the download carries SubmittedBy (resolved operator UserId)
        //  for j07-btrade-sync to relay. §10 PascalCase payload fields, matching
        //  the SessionController convention; the JSend envelope keys stay
        //  lowercase by declaration on JSendOk.
        return new JsonResult(new JSendOk(result))
        {
            StatusCode = StatusCodes.Status200OK,
            SerializerSettings = new JsonSerializerOptions()
        };
    }
}

public record ReturnOrderSubmitRequest(
    string ReturnOrderId,
    string ReturnOrderDate,
    string WarehouseCode,
    string CustomerId,
    string CustomerName,
    string SalesPersonId,
    string SalesPersonName,
    string DriverId,
    string DriverName,
    string Note,
    IEnumerable<ReturnOrderItemType> ListItem);

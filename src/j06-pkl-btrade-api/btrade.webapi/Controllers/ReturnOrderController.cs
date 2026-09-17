using btrade.application.UseCase;
using btrade.domain.ReturnOrderFeature;
using btrade.webapi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReturnOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("~/api/return-order")]
    public async Task<IActionResult> Submit(ReturnOrderSubmitRequest request)
    {
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
            User.GetServerId(),
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
        return Ok(new JSendOk(result));
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

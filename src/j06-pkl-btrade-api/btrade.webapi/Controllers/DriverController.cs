using btrade.application.UseCase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

//  TD-06 — [Authorize] is removed instance-wide. Both routes keep their
//  existing contract: GET api/Driver/{serverId} is a legacy read route with a
//  client-supplied serverId (TD-08), and POST api/Driver keeps its body-bound
//  DriverSyncCommand for token-bearing consumers (j07-btrade-sync); neither
//  reads JWT claims.
[Route("api/[controller]")]
[ApiController]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriverController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("{serverId}")]
    public async Task<IActionResult> ListData(string serverId)
    {
        var query = new DriverListDataQuery(serverId);
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }

    [HttpPost]
    public async Task<IActionResult> SyncData(DriverSyncCommand cmd)
    {
        var response = await _mediator.Send(cmd);
        return Ok(new JSendOk(response));
    }
}

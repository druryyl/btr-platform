using btrade.application.UseCase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesPersonController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesPersonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("{serverId}")]
    public async Task<IActionResult> ListData(string serverId)
    {
        var query = new SalesPersonListDataQuery(serverId);
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }

    [HttpPost]
    public async Task<IActionResult> SyncData(SalesPersonSyncCommand cmd)
    {
        var response = await _mediator.Send(cmd);
        return Ok(new JSendOk(response));
    }

    [HttpGet]
    [Route("exists")]
    public async Task<IActionResult> IsValidEmail([FromQuery] string email)
    {
        var query = new IsValidSalesEmailQuery(email);
        var response = await _mediator.Send(query);
        return Ok(new JSendOk(response));
    }
}

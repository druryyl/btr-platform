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
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SyncData(UserSyncRequest request)
    {
        var cmd = new UserSyncCommand(
            request.ListUser ?? Enumerable.Empty<UserType>(),
            User.GetServerId());
        await _mediator.Send(cmd);
        return Ok(new JSendOk("Done"));
    }
}

public record UserSyncRequest(IEnumerable<UserType> ListUser);

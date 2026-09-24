using System.Text.Json;
using btrade.application.UseCase;
using Microsoft.AspNetCore.Mvc;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Controllers;

/// <summary>
/// TD-02 — anonymous Cloud account resolution for BGud sign-in. The endpoint
/// issues no token, requires no authentication, and stores no server-side
/// session state: BGud remains stateless towards the Cloud between requests.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly SessionContextResolver _sessionContextResolver;

    public SessionController(SessionContextResolver sessionContextResolver)
    {
        _sessionContextResolver = sessionContextResolver;
    }

    [HttpPost]
    [Route("~/api/session/resolve")]
    public IActionResult Resolve(SessionResolveRequest request)
    {
        try
        {
            var account = _sessionContextResolver.ResolveAccount(request.Email);
            var warehouses = _sessionContextResolver
                .ListWarehouses()
                .Select(mapping => new SessionWarehouseDto(
                    mapping.WarehouseCode,
                    mapping.ServerId))
                .ToList();
            var response = new SessionResolveResponse(
                account.UserId,
                account.UserName,
                account.RoleId,
                warehouses);

            //  §10: PascalCase payload fields inside the JSendOk envelope. The
            //  envelope keys (status/code/data) are lowercase by declaration on
            //  JSendOk itself and are unaffected by the naming policy.
            return new JsonResult(new JSendOk(response))
            {
                StatusCode = StatusCodes.Status200OK,
                SerializerSettings = new JsonSerializerOptions()
            };
        }
        catch (SessionAccountUnresolvableException ex)
        {
            //  §9 Error Semantics — an unmapped or invalid Google account is a
            //  sign-in refusal: HTTP 400 with the same JSend failure body the
            //  ErrorHandlerMiddleware emits for bad requests.
            return BadRequest(new JSend(400, "Bad Request", ex.Message));
        }
    }
}

public record SessionResolveRequest(string Email);

public record SessionResolveResponse(
    string UserId,
    string UserName,
    string RoleId,
    IEnumerable<SessionWarehouseDto> Warehouses);

public record SessionWarehouseDto(string LocationId, string ServerId);

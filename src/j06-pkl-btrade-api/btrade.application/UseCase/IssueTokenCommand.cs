using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace btrade.application.UseCase;

public record IssueTokenCommand(
    string UserId,
    string Password,
    string LocationId) : IRequest<IssueTokenResult>;

public record IssueTokenResult(
    string Token,
    DateTime ExpiresAt,
    string UserId,
    string UserName,
    string RoleId,
    string LocationId,
    string ServerId);

public class IssueTokenCommandHandler
    : IRequestHandler<IssueTokenCommand, IssueTokenResult>
{
    private readonly IUserDal _userDal;
    private readonly ILocationDal _locationDal;
    private readonly IJwtTokenService _jwtTokenService;

    public IssueTokenCommandHandler(
        IUserDal userDal,
        ILocationDal locationDal,
        IJwtTokenService jwtTokenService)
    {
        _userDal = userDal;
        _locationDal = locationDal;
        _jwtTokenService = jwtTokenService;
    }

    public Task<IssueTokenResult> Handle(IssueTokenCommand request, CancellationToken cancellationToken)
    {
        //  GAP-008 / IR-05 — verify the SHA-256 hash against the credential
        //  projection exactly as BTR Desktop does (LoginForm.cs).
        var user = _userDal.GetData(new UserKey(request.UserId ?? string.Empty));
        if (!user.HasValue ||
            HashSha256(request.Password ?? string.Empty) != user.Value.Password)
            throw new UnauthorizedAccessException("Invalid credentials");

        //  ADR-007 / 9.1 — ServerId is resolved server-side from the selected
        //  operational location; it is never supplied by the client.
        var location = _locationDal.GetData(new LocationKey(request.LocationId ?? string.Empty));
        if (!location.HasValue)
            throw new ArgumentException($"Invalid location ({request.LocationId})");

        var token = _jwtTokenService.GenerateToken(
            user.Value.UserId,
            user.Value.RoleId,
            location.Value.LocationId,
            location.Value.ServerId);

        return Task.FromResult(new IssueTokenResult(
            token.Token,
            token.ExpiresAt,
            user.Value.UserId,
            user.Value.UserName,
            user.Value.RoleId,
            location.Value.LocationId,
            location.Value.ServerId));
    }

    private static string HashSha256(string password)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var hashByte in hashBytes)
            builder.Append(hashByte.ToString("x2"));
        return builder.ToString();
    }
}

public record UserKey(string UserId) : IUserKey;

public record LocationKey(string LocationId) : ILocationKey;

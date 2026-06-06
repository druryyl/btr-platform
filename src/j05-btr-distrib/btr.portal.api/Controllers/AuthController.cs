using System.Net;
using System.Web.Http;
using btr.application.SupportContext.UserAgg;
using btr.domain.SupportContext.UserAgg;
using btr.nuna.Domain;
using btr.portal.api.Infrastructure;
using btr.portal.api.Models;
using NLog;

namespace btr.portal.api.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUserDal _userDal;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IUserDal userDal, IJwtTokenService jwtTokenService)
        {
            _userDal = userDal;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost, Route("login"), AllowAnonymous]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Error(400, "UserId and password are required."));
            }

            var user = _userDal.GetData(new UserModel(request.UserId)) ?? new UserModel();
            user.RemoveNull();

            var passHash = request.Password.HashSha256();
            if (passHash != user.Password || string.IsNullOrEmpty(user.UserId))
            {
                Logger.Warn("Failed login attempt for user {UserId}", request.UserId);
                return Content(
                    HttpStatusCode.Unauthorized,
                    ApiResponse<object>.Error(401, "Invalid credentials"));
            }

            Logger.Info("Successful login for user {UserId}", user.UserId);

            var tokenResult = _jwtTokenService.GenerateToken(
                user.UserId,
                user.UserName,
                user.RoleId,
                user.RoleName);
            var response = new LoginResponse
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAtUtc,
                User = new LoginUserInfo
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName
                }
            };

            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [HttpGet, Route("me"), Authorize]
        public IHttpActionResult Me()
        {
            var identity = User?.Identity as System.Security.Claims.ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    ApiResponse<object>.Error(401, "Invalid credentials"));
            }

            var user = new LoginUserInfo
            {
                UserId = identity.FindFirst("userId")?.Value ?? identity.Name,
                UserName = identity.FindFirst("userName")?.Value ?? string.Empty,
                RoleId = identity.FindFirst("roleId")?.Value ?? string.Empty,
                RoleName = identity.FindFirst("roleName")?.Value ?? string.Empty
            };

            return Ok(ApiResponse<LoginUserInfo>.Success(user));
        }
    }
}

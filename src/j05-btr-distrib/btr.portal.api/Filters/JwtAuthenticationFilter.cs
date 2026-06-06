using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using btr.portal.api.Infrastructure;
using btr.portal.api.Models;

namespace btr.portal.api.Filters
{
    public class JwtAuthenticationFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (IsAllowAnonymous(actionContext))
                return;

            if (!RequiresAuthorization(actionContext))
                return;

            var authorizationHeader = actionContext.Request.Headers.Authorization;
            if (authorizationHeader == null ||
                !string.Equals(authorizationHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(authorizationHeader.Parameter))
            {
                Deny(actionContext, "Missing or invalid authorization token.");
                return;
            }

            var dependencyScope = actionContext.Request.GetDependencyScope();
            var tokenService = dependencyScope.GetService(typeof(IJwtTokenService)) as IJwtTokenService;
            if (tokenService == null)
            {
                Deny(actionContext, "Authentication service is unavailable.");
                return;
            }

            if (!tokenService.TryValidateToken(authorizationHeader.Parameter, out var principal))
            {
                Deny(actionContext, "Invalid or expired token.");
                return;
            }

            SetPrincipal(principal);
        }

        private static bool IsAllowAnonymous(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
                   actionContext.ControllerContext.ControllerDescriptor
                       .GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }

        private static bool RequiresAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>().Any() ||
                   actionContext.ControllerContext.ControllerDescriptor
                       .GetCustomAttributes<AuthorizeAttribute>().Any();
        }

        private static void SetPrincipal(ClaimsPrincipal principal)
        {
            var authenticatedPrincipal = principal.Identity?.IsAuthenticated == true
                ? principal
                : new ClaimsPrincipal(new ClaimsIdentity(principal.Claims, "Jwt"));

            Thread.CurrentPrincipal = authenticatedPrincipal;

            if (HttpContext.Current != null)
                HttpContext.Current.User = authenticatedPrincipal;
        }

        private static void Deny(HttpActionContext actionContext, string message)
        {
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Error(401, message));
        }
    }
}

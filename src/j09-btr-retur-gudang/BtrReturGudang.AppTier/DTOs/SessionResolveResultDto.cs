using System.Collections.Generic;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Rich result model for Cloud operator session resolution (<c>POST api/session/resolve</c>).
    /// Carries the HTTP status code, success flag, error message, the resolved BTR identity
    /// (<see cref="UserId"/>/<see cref="UserName"/>/<see cref="RoleId"/>), the warehouse
    /// mapping, and the raw response payload so callers can surface detailed diagnostics for
    /// an unmapped or inactive Google account.
    /// Traceable to: ARCHITECTURE §5.1, §6, TD-012.
    /// </summary>
    public class SessionResolveResultDto
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public IReadOnlyList<SessionWarehouseDto> Warehouses { get; set; }
        public string RawResponse { get; set; }

        public static SessionResolveResultDto Ok(
            int statusCode = 200,
            string rawResponse = null,
            string userId = null,
            string userName = null,
            string roleId = null,
            IReadOnlyList<SessionWarehouseDto> warehouses = null)
        {
            return new SessionResolveResultDto
            {
                Success = true,
                StatusCode = statusCode,
                ErrorMessage = null,
                UserId = userId,
                UserName = userName,
                RoleId = roleId,
                Warehouses = warehouses,
                RawResponse = rawResponse
            };
        }

        public static SessionResolveResultDto Fail(int statusCode, string errorMessage, string rawResponse = null)
        {
            return new SessionResolveResultDto
            {
                Success = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                RawResponse = rawResponse
            };
        }
    }
}

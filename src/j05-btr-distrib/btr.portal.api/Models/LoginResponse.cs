using System;

namespace btr.portal.api.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public LoginUserInfo User { get; set; }
    }

    public class LoginUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}

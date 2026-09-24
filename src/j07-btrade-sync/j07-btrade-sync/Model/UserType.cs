using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class UserType
    {
        public UserType(string userId, string userName, string password, string roleId,
            string email, bool isAktif, string serverId)
        {
            UserId = userId;
            UserName = userName;
            Password = password;
            RoleId = roleId;
            Email = email;
            IsAktif = isAktif;
            ServerId = serverId;
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RoleId { get; set; }
        //  TD-14 — the Google-email mapping replicated from BTR_User.Email.
        public string Email { get; set; }
        public bool IsAktif { get; set; }
        public string ServerId { get; set; }
    }
}

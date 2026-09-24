using System.Collections.Generic;
using btr.application.SupportContext.UserAgg;
using btr.domain.SupportContext.UserAgg;
using btr.nuna.Infrastructure;

namespace btr.test.SupportContext
{
    internal class UserDalStub : IUserDal
    {
        public List<UserModel> Users { get; } = new List<UserModel>();

        public void Insert(UserModel model)
        {
            Users.Add(model);
        }

        public void Update(UserModel model)
        {
        }

        public void Delete(IUserKey key)
        {
            Users.RemoveAll(x => x.UserId == key.UserId);
        }

        public UserModel GetData(IUserKey key)
        {
            return Users.Find(x => x.UserId == key.UserId);
        }

        public IEnumerable<UserModel> ListData()
        {
            return Users;
        }
    }
}

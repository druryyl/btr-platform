using btr.application.SupportContext.UserAgg;
using btr.domain.SupportContext.UserAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SupportContext
{
    public class UserBuilderTest
    {
        [Fact]
        public void Email_TrimsInputValue_OnBuild()
        {
            var builder = new UserBuilder(new UserDalStub());

            var user = builder
                .LoadOrCreate(UserModel.Key("U1"))
                .Email("  operator@gmail.com  ")
                .Build();

            user.Email.Should().Be("operator@gmail.com");
        }

        [Fact]
        public void Email_Null_BecomesEmpty_OnBuild()
        {
            var builder = new UserBuilder(new UserDalStub());

            var user = builder
                .LoadOrCreate(UserModel.Key("U1"))
                .Email(null)
                .Build();

            user.Email.Should().Be(string.Empty);
        }
    }
}

using System;
using btr.application.SupportContext.UserAgg;
using btr.domain.SupportContext.UserAgg;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace btr.test.SupportContext
{
    public class UserValidatorTest
    {
        private readonly UserDalStub _userDal;
        private readonly UserValidator _validator;

        public UserValidatorTest()
        {
            _userDal = new UserDalStub();
            _userDal.Users.Add(ValidUser("U1"));
            _userDal.Users[0].Email = "operator@gmail.com";
            _userDal.Users.Add(ValidUser("U2"));
            _validator = new UserValidator(_userDal);
        }

        [Fact]
        public void Validate_Rejects_DuplicateEmail_OfAnotherUser_IgnoringCase()
        {
            var model = ValidUser("U3");
            model.Email = "OPERATOR@GMAIL.COM";

            Action act = () => _validator.ValidateAndThrow(model);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Email is already registered to another user*");
        }

        [Fact]
        public void Validate_Rejects_DuplicateEmail_ComparedTrimmed()
        {
            _userDal.Users[0].Email = "operator@gmail.com  ";
            var model = ValidUser("U3");
            model.Email = " Operator@Gmail.Com ";

            Action act = () => _validator.ValidateAndThrow(model);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Email is already registered to another user*");
        }

        [Fact]
        public void Validate_Allows_KeepingOwnEmail()
        {
            var model = ValidUser("U1");
            model.Email = "operator@gmail.com";

            var result = _validator.Validate(model);

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_Allows_UniqueEmail()
        {
            var model = ValidUser("U3");
            model.Email = "other@gmail.com";

            var result = _validator.Validate(model);

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_Allows_EmptyEmail_MultipleUnmappedUsers()
        {
            var model = ValidUser("U3");
            model.Email = string.Empty;

            var result = _validator.Validate(model);

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_Allows_WhitespaceEmail_TreatedAsUnmapped()
        {
            var model = ValidUser("U3");
            model.Email = "   ";

            var result = _validator.Validate(model);

            result.Errors.Should().BeEmpty();
        }

        private static UserModel ValidUser(string userId)
        {
            return new UserModel
            {
                UserId = userId,
                UserName = "User Name",
                Password = "hash",
                Prefix = "A",
                RoleId = "ADM",
                Email = string.Empty
            };
        }
    }
}

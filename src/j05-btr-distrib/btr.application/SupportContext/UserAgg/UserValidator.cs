using System;
using System.Collections.Generic;
using System.Linq;
using btr.domain.SupportContext.UserAgg;
using FluentValidation;

namespace btr.application.SupportContext.UserAgg
{
    public class UserValidator : AbstractValidator<UserModel>
    {
        private readonly IUserDal _userDal;

        public UserValidator() : this(null)
        {
        }

        public UserValidator(IUserDal userDal)
        {
            _userDal = userDal;

            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Email)
                .Must(BeUniqueEmail)
                .WithMessage("Email is already registered to another user");
        }

        private bool BeUniqueEmail(UserModel model, string email)
        {
            if (_userDal is null)
                return true;

            var normalized = (email ?? string.Empty).Trim();
            if (normalized.Length == 0)
                return true;

            var otherUsers = _userDal.ListData() ?? Enumerable.Empty<UserModel>();
            return !otherUsers.Any(x =>
                x != null &&
                x.UserId != model.UserId &&
                string.Equals((x.Email ?? string.Empty).Trim(), normalized,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}

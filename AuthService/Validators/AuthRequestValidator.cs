using AuthService.Contracts.Request;
using FluentValidation;

namespace AuthService.Validators
{
    /// <summary>
    /// FluentValidation validator for login requests.
    /// Keeps validation rules out of the controller.
    /// </summary>
    public class AuthRequestValidator : AbstractValidator<AuthRequest>
    {
        public AuthRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}


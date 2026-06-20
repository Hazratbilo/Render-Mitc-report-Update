using FluentValidation;

namespace MITCRMS.Models.DTOs.Users.Validation
{
    public class CompleteStaffSignupValidation : AbstractValidator<CompleteStaffSignupRequestModel>
    {
        public CompleteStaffSignupValidation()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Signup token is required.");
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(50).WithMessage("Password must not exceed 50 characters.")
                .Must(p => p.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
                .Must(p => p.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
                .Must(p => p.Any(char.IsDigit)).WithMessage("Password must contain at least one digit.")
                .Must(p => p.Any(ch => !char.IsLetterOrDigit(ch))).WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Confirm password must match the password.");
        }
    }
}

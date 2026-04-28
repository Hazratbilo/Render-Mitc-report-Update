using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Implementation.Repository;
using MITCRMS.Persistence.Context;
using System;

namespace MITCRMS.Models.DTOs.Users.Validation
{
    public class CreateUserValidation: AbstractValidator<CreateUserRequestModel>
    {
        public CreateUserValidation()
        { 
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone Number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.PasswordHash)
                 .NotEmpty().WithMessage("Password is required.")
                 .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                 .MaximumLength(50).WithMessage("Password must not exceed 50 characters.")
                 .Must(p => p.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
                 .Must(p => p.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
                 .Must(p => p.Any(char.IsDigit)).WithMessage("Password must contain at least one digit.")
                 .Must(p => p.Any(char.IsSymbol)).WithMessage("Password must contain at least one special character.");

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Confirm password is required.")
                    .Equal(x => x.PasswordHash).WithMessage("Confirm password must match the password.");
                RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("Department is required");
            RuleFor(x => x.RoleIds).NotEmpty().WithMessage("At least one role must be selected");
        }
    }
}

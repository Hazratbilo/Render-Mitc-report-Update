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

            RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("Department is required");
            RuleFor(x => x.RoleIds).NotEmpty().WithMessage("At least one role must be selected");
        }
    }
}

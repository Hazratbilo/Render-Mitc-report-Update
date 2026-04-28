using FluentValidation;

namespace MITCRMS.Models.DTOs.Department.Validation
{
    public class CreateDepartmentValidation : AbstractValidator<CreateDepartmentRequestModel>
    {
        public CreateDepartmentValidation()
        {
            RuleFor(x => x.DepartmentName).Length(3, 50).NotEmpty().WithMessage("DepartmentName is required");
            RuleFor(x => x.DepartmentCode).Length(3, 50).NotEmpty().WithMessage("DepartmentCode is required");
        }
    }
}

using FluentValidation;

namespace MITCRMS.Models.DTOs.Report.Validation
{
    public class CreateReportValidation : AbstractValidator<CreateReportRequestModel>
    {
        public CreateReportValidation()
        {
            RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required");
            RuleFor(x => x.Tittle).NotEmpty().WithMessage("Tittle is required");
            RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("DepartmentId is required");
        }
    }
}
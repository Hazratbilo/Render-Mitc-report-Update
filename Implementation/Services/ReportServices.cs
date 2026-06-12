
using Microsoft.EntityFrameworkCore;
using Mitc_report_Update.Interface.MailingService;
using MITCRMS.Implementation.Repository;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.Entities;
using MITCRMS.Models.Enum;
using MITCRMS.Persistence.Context;



namespace MITCRMS.Implementation.Services
{
    public class ReportServices : IReportServices
    {
        private readonly MitcrmsContext _MitcrmsContext;
        private readonly IReportRepository _reportRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportServices> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMailService _mailService;

        public ReportServices(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IDepartmentRepository departmentRepository,
            ILogger<ReportServices> logger,  MitcrmsContext MitcrmsContext,
            IMailService mailService)
        {
            _reportRepository = reportRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _MitcrmsContext = MitcrmsContext;
            _mailService = mailService;
        }

        public async Task<BaseResponse<bool>> AcceptReport(Guid id)
        {
            var report = await _reportRepository.Get<Report>(r => r.Id == id);
            if (report == null)
            {
                _logger.LogError($"Report with Id: '{id}' cannot be found");
                return new BaseResponse<bool>
                {
                    Message = $"Report with Id: '{id}' cannot be found",
                    Status = false
                };
            }

            if (report.Status == ReportStatus.Rejected)
            {
                _logger.LogError($"Report with Id: '{id}' has already been rejected and cannot be approved");
                return new BaseResponse<bool>
                {
                    Message = "Rejected reports cannot be approved",
                    Status = false
                };
            }


            var success = _reportRepository.AcceptReport(report);
            if (!success)
            {
                _logger.LogError("Report couldn't be approved");
                return new BaseResponse<bool>
                {
                    Message = "Report couldn't be approved",
                    Status = false
                };
            }

            return new BaseResponse<bool>
            {
                Message = "Report approved",
                Status = true
            };
        }

        public async Task<BaseResponse<bool>> CreateReportAsync(string fileUrl,
       CreateReportRequestModel request,
       Guid loggedInUserId,
       string role)
        {
            if (fileUrl is null && request.Content is null)
                return new BaseResponse<bool> { Status = false, Message = "Content and file cannot be empty" };

            var normalizedRole = role.Trim().ToLowerInvariant();

            var user = await _userRepository.Get<User>(u => u.Id == loggedInUserId);
            if (user == null)
            {
                return new BaseResponse<bool> { Status = false, Message = "User not found" };
            }

            var report = new Report
            {

                UserID = user?.Id,
                Tittle = request.Tittle,
                Content = request.Content,
                FileUrl = fileUrl ?? string.Empty,
                Status = ReportStatus.Pending,
                DateCreated = DateTime.UtcNow
            };

            await _reportRepository.Add(report);
            var result = await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return result > 0 ? new BaseResponse<bool>
            {
                Status = true,
                Message = "Report created successfully"
            }
            : new BaseResponse<bool> { Status = false, Message = "Report submission failed" };
        }
        public async Task<BaseResponse<IReadOnlyList<ReportDto>>> GetCancelledReportsAsync(CancellationToken cancellationToken)
        {
            var reports = await _reportRepository.GetAllCancelledReport();
            if (reports == null || !reports.Any())
            {
                return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "No reports found", Status = false, Data = Array.Empty<ReportDto>() };
            }

            var data = reports.Select(MapToDto).ToList();
            return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "Data fetched successfully", Status = true, Data = data };
        }

        public async Task<BaseResponse<IReadOnlyList<ReportDto>>> GetCompletedReportsAsync(CancellationToken cancellationToken)
        {
            var reports = await _reportRepository.GetAllCompletedReport();
            if (reports == null || !reports.Any())
            {
                return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "No reports found", Status = false, Data = Array.Empty<ReportDto>() };
            }

            var data = reports.Select(MapToDto).ToList();
            return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "Data fetched successfully", Status = true, Data = data };
        }

        public async Task<BaseResponse<ReportDto>> GetReportByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var report = await _reportRepository.GetRepordById(id);
            if (report == null)
            {
                return new BaseResponse<ReportDto> { Message = "Report not found", Status = false };
            }

            return new BaseResponse<ReportDto> { Message = "Report fetched", Status = true, Data = MapToDto(report) };
        }

        public async Task<BaseResponse<bool>> UpdateReport(Guid id, CreateReportRequestModel request)
        {
            if (request == null)
                return new BaseResponse<bool> { Message = "Invalid request", Status = false };

            var report = await _reportRepository.Get<Report>(r => r.Id == id);
            if (report == null)
                return new BaseResponse<bool> { Message = "Report not found", Status = false };

            if (report.Status == ReportStatus.Approved)
                return new BaseResponse<bool> { Message = "Approved reports cannot be edited", Status = false };

            report.Tittle = string.IsNullOrWhiteSpace(request.Tittle) ? report.Tittle : request.Tittle;
            report.Content = string.IsNullOrWhiteSpace(request.Content) ? report.Content : request.Content;
         
            _reportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool> { Message = "Report updated", Status = true };
        }
        private static ReportDto MapToDto(Report r)
        {
#pragma warning disable CS8601
#pragma warning disable CS8602 
            return new ReportDto
            {
                Id = r.Id,
                DateCreated = r.DateCreated,
                Tittle = r.Tittle,
                Content = r.Content,
                Status = r.Status,
                ApprovedAt = r.ApprovedAt,
                FileUrl= r.FileUrl,
                //ApprovedByAdminId = r.ApprovedByAdminId,
                UserId = r.UserID,
                User = r.User != null ? new Models.DTOs.Users.UserDto
                {
                    Id = r.User.Id,
                    FullName = r.User.FullName(),
                    Email = r.User?.Email,
                    PhoneNumber = r.User.PhoneNumber,
                    UserId = r.User.Id,
                } : null,
            };
#pragma warning restore CS8602
#pragma warning restore CS8601 
        }
        public async Task<IEnumerable<Report>> GetReportsByUserAsync(Guid UserId, string role)
        {
            IEnumerable<Report> reports = new List<Report>();


            var report = await _userRepository.GetAll<User>();
            return reports;
        }



        public async Task<BaseResponse<IReadOnlyList<ReportDto>>> GetMyReportsAsync(Guid UserId)
        {
            var getAllReports = await _reportRepository.GetMyReports(r => r.User.Id == UserId);
            if (!getAllReports.Any())
            {
                _logger.LogError($"No Report found");
                return new BaseResponse<IReadOnlyList<ReportDto>>
                {
                    Message = $"No Report found",
                    Status = false,
                };
            }
            _logger.LogInformation("All Report fetched successfully");
            return new BaseResponse<IReadOnlyList<ReportDto>>
            {
                Message = "All Report fetched successfully",
                Status = true,
                Data = getAllReports.Select(dpt => new ReportDto
                {
                    Id = dpt.Id,
                    Tittle = dpt.Tittle,
                    Content = dpt.Content,
                    FileUrl = dpt.FileUrl,
                    DateCreated = dpt.DateCreated,
                    Status = dpt.Status,
                }).ToList()
            };
        }
        public async Task<BaseResponse<ReportDto>> GetReportById(Guid id)
        {
            var getNote = await _reportRepository.GetReportById(id);
            if (getNote == null)
            {
                _logger.LogError($"Report with id {id} not found");
                return new BaseResponse<ReportDto>
                {
                    Message = $"Report with id {id} not found",
                    Status = false,
                };
            }
            _logger.LogInformation("Report fetched successfully");
            return new BaseResponse<ReportDto>
            {
                Message = "Report fetched successfully",
                Status = true,
                Data = new ReportDto
                {
                    Id = getNote.Id,
                    Tittle = getNote.Tittle,
                    FullName = getNote.User != null ? getNote.User.FullName() : string.Empty,
                    DepartmentName = getNote.User?.Department?.DepartmentName ?? string.Empty,
                    Content = getNote.Content,
                    FileUrl = getNote.FileUrl,
                    DateCreated = getNote.DateCreated,

                }
            };
        }
        public async Task<BaseResponse<bool>> DeleteReport(Guid id)
        {
            var getReport = await _reportRepository.GetReportById(id);
            if (getReport == null)
            {
                _logger.LogError($"Report with id {id} not found");
                return new BaseResponse<bool>
                {
                    Message = $"Report with id {id} not found",
                    Status = false
                };
            }

            var deleteReport = await _reportRepository.DeleteReport(getReport);
            if (deleteReport)
            {
                _logger.LogInformation("Delete Report Success");
                return new BaseResponse<bool>
                {
                    Message = "Delete Report Success",
                    Status = true
                };
            }
            _logger.LogError("Delete Report Failed");
            return new BaseResponse<bool>
            {
                Message = "Delete Report Failed",
                Status = false
            };
        }

        public async Task<BaseResponse<IReadOnlyList<ReportDto>>> GetUserReportByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var reports = await _reportRepository.GetReportsByUserId(userId);
            if (reports == null || !reports.Any())
            {
                _logger.LogInformation("No reports found for User {UserId}", userId);
                return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "No report found", Status = false, Data = Array.Empty<ReportDto>() };
            }

            var data = reports.Select(ap => MapToDto(ap)).ToList();
            return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "Data fetched successfully", Status = true, Data = data };
        }

        public async Task<BaseResponse<IReadOnlyList<ReportDto>>> GetAllReportsByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken)
        {

           var dept =  await _departmentRepository.Get<Department>(d => d.Id == departmentId);
            if (dept is null)
            {
                _logger.LogError("Department cannot be found");
                return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "Department cannot be found", Status = false, Data = Array.Empty<ReportDto>() };
            }

            var reports = await _reportRepository.GetAllReportsByDepartmentId(dept.Id);
            if (reports == null || !reports.Any())
            {
                _logger.LogInformation("No reports found for Department {DepartmentId}", departmentId);
                return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "No report found", Status = false, Data = Array.Empty<ReportDto>() };
            }
            var data = reports.Select(ap => MapToDto(ap)).ToList();
            return new BaseResponse<IReadOnlyList<ReportDto>> { Message = "Data fetched successfully", Status = true, Data = data };
        }

        public async Task<List<ReportDto>> GetAllReportsAsync()
        {
            var reports = await _reportRepository.GetAllReportAsync();

            return reports.Select(r => new ReportDto
            {
                Id = r.Id,
                Tittle = r.Tittle,
                FullName = r.User != null ? r.User.FullName() : string.Empty,
                DepartmentName = r.User?.Department?.DepartmentName ?? string.Empty,
                Content = r.Content,
                FileUrl = r.FileUrl,
                DateCreated = r.DateCreated,
                Status = r.Status,
            }).ToList();
        }
       public async Task<bool> ChangeReportStatusAsync(Guid id, ReportStatus status)
{
    var report = await _MitcrmsContext.Set<Report>()
        .Include(r => r.User) 
        .FirstOrDefaultAsync(r => r.Id == id);

    if (report == null)
        return false;

    if (report.Status == ReportStatus.Rejected && status == ReportStatus.Approved)
    {
        _logger.LogError($"Report with Id: '{id}' has already been rejected and cannot be approved");
        return false;
    }

    if (report.Status == ReportStatus.Approved && status == ReportStatus.Rejected)
    {
        _logger.LogError($"Report with Id: '{id}' has already been approved and cannot be rejected");
        return false;
    }

    report.Status = status;

    await _MitcrmsContext.SaveChangesAsync();

    // Send Email 
    await _mailService.SendReportStatus(
        report.User.Email,
        report.User.FullName(),
        report.Tittle,
        status.ToString()
        
    );

    return true;
}
    }
    }

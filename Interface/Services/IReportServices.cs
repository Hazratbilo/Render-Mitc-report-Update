using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.DTOs.Role;
using MITCRMS.Models.Entities;
using MITCRMS.Models.Enum;
using System.Threading;

namespace MITCRMS.Interface.Services
{
    public interface IReportServices
    {
        Task<BaseResponse<bool>> CreateReportAsync(string fileUrl, CreateReportRequestModel request, Guid loggedInUserId, string role);

        Task<IEnumerable<Report>> GetReportsByUserAsync(Guid userId, string role);
        Task<BaseResponse<ReportDto>> GetReportByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<BaseResponse<IReadOnlyList<ReportDto>>> GetMyReportsAsync(Guid userId);
        Task<BaseResponse<IReadOnlyList<ReportDto>>> GetUserReportByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<BaseResponse<bool>> AcceptReport(Guid id);
        Task<BaseResponse<IReadOnlyList<ReportDto>>> GetAllReportsByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken);
        Task<BaseResponse<IReadOnlyList<ReportDto>>> GetCancelledReportsAsync(CancellationToken cancellationToken);
        Task<BaseResponse<IReadOnlyList<ReportDto>>> GetCompletedReportsAsync(CancellationToken cancellationToken);
        public Task<BaseResponse<bool>> DeleteReport(Guid id);
        public Task<BaseResponse<ReportDto>> GetReportById(Guid id);
        Task<List<ReportDto>> GetAllReportsAsync();
        //public Task<BaseResponse<ReportDto>> GetAllReportsById(Guid id);
        Task<bool> ChangeReportStatusAsync(Guid id, ReportStatus newStatus);
    }
}

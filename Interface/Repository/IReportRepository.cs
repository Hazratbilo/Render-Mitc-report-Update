using MITCRMS.Contract.Entity;
using MITCRMS.Models.Entities;
using System.Linq.Expressions;

namespace MITCRMS.Interface.Repository
{
    public interface IReportRepository:IBaseRepository
    {
        public Task<Report> AddReport(Report report);
        Task<IEnumerable<Report>> GetAll(Expression<Func<Report, bool>> predicate);
        Task<IReadOnlyList<Report>> GetAllReportsByDepartmentId(Guid departmentId);
        Task<IReadOnlyList<Report>> GetAllCancelledReport();
        Task<IReadOnlyList<Report>> GetAllCompletedReport();
        bool AcceptReport(Report report);
        Task<IReadOnlyList<Report>> GetReportsByUserId(Guid userId);
        Task<Report> GetRepordById(Guid id);
        public Task<bool> DeleteReport(Report report);
        public Task<Report> GetReportById(Guid id);
        public Task<Report> GetAllReportsById(Guid id);
        Task UpdateAsync(Report report);
        Task<IReadOnlyList<Report>> GetMyReports(Expression<Func<Report, bool>> expression);
        Task<List<Report>> GetAllReportAsync();
        Task<List<User>> GetStaffWithoutReportWeek();


    }

}

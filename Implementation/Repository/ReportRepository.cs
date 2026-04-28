using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Contract.Entity;
using MITCRMS.Interface.Repository;
using MITCRMS.Models.Entities;
using MITCRMS.Models.Enum;
using MITCRMS.Persistence.Context;
using MITCRMS.Persistence.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MITCRMS.Implementation.Repository
{
    public class ReportRepository : BaseRepository, IReportRepository
    {
        public ReportRepository(MitcrmsContext mitcrmsContext) : base(mitcrmsContext)
        {
        }

        public bool AcceptReport(Report report)
        {
            if (report == null) return false;

            try
            {
                report.Status = ReportStatus.Approved;
                report.ApprovedAt = DateTime.UtcNow;

                _mitcrmsContext.Set<Report>().Update(report);
                _mitcrmsContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<Report>> GetAllCancelledReport()
        {
            return await _mitcrmsContext.Set<Report>()
                .Where(r => r.Status == ReportStatus.Rejected)
                .OrderByDescending(r => r.DateCreated)
               .Include(r => r.User)
               .ThenInclude(u => u.Department)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Report>> GetAllCompletedReport()
        {
            return await _mitcrmsContext.Set<Report>()
                .Where(r => r.Status == ReportStatus.Approved)
                .OrderByDescending(r => r.DateCreated)
                 .Include(r => r.User)
               .ThenInclude(u => u.Department)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Report>> GetAllReport()
        {
            return await _mitcrmsContext.Set<Report>()
                .OrderByDescending(r => r.DateCreated)
                 .Include(r => r.User)
               .ThenInclude(u => u.Department)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Report> GetRepordById(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<Report>()
                .Where(r => r.Id == id)
                .Include(r => r.User)
               .ThenInclude(u => u.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

   
        public async Task<IEnumerable<Report>> GetAll(Expression<Func<Report, bool>> predicate)
        {
            return await _mitcrmsContext.Set<Report>()
                .Where(predicate)
                .Include(r => r.User)
               .ThenInclude(u => u.Department)
                                 .ToListAsync();
        }

        public async Task<Report> AddReport(Report report)
        {
            await _mitcrmsContext.Set<Report>().AddAsync(report);
            return report;
        }
        public async Task<IEnumerable<Report>> GetAllReports()
        {
            var report = await _mitcrmsContext.Set<Report>().ToListAsync();
              return report;
        }

        public async Task<IReadOnlyList<Report>> GetMyReports(Expression<Func<Report, bool>> expression)
        {
            return await _mitcrmsContext.Set<Report>()
                .Where(expression)
                .Include(r => r.User)
                .OrderByDescending(r => r.DateCreated)
                .AsSplitQuery()
                .ToListAsync();
        }
        public async Task<Report> GetReportById(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<Report>().FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8603 // Possible null reference return.

        }
        public async Task<Report> GetAllReportsById(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<Report>().FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8603 // Possible null reference return.

        }
        public async Task<bool> DeleteReport(Report report)
        {
            _mitcrmsContext.Set<Report>().Remove(report);
           return await _mitcrmsContext.SaveChangesAsync() > 0 ? true : false;
         
        }

        public async Task<IReadOnlyList<Report>> GetReportsByUserId(Guid userId)
        {
            var reportsByAuthor = await _mitcrmsContext.Set<Report>()
               .Where(r => r.UserID == userId)
               .OrderByDescending(r => r.DateCreated)
                .Include(r => r.User)
               .ThenInclude(u => u.Department)
               .AsNoTracking()
               .ToListAsync();

            return reportsByAuthor;
        }

        public async Task<IReadOnlyList<Report>> GetAllReportsByDepartmentId(Guid departmentId)
        {
            var departmentReport = await _mitcrmsContext.Set<Report>()
                .Include(r => r.User)
                .AsNoTracking()
                .ToListAsync();

            return departmentReport.Where(r => r.User.DepartmentId == departmentId)
                .OrderByDescending(r => r.DateCreated)
                .ToList();
        }

        public async Task<List<Report>> GetAllReportAsync()
        {
            return await _mitcrmsContext.Set<Report>()
                .Include(r => r.User)
                .ThenInclude(u => u.Department)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }
        public async Task UpdateAsync(Report report)
        {
            _mitcrmsContext.Set<Report>().Update(report);
            await _mitcrmsContext.SaveChangesAsync();
        }

        public async Task<List<User>> GetStaffWithoutReportWeek()
        {
            var (year, weekNumber) = ReportingWeekHelper.GetCurrentReportingWeek();

            return await _mitcrmsContext.Set<User>()
                .Where(u => !_mitcrmsContext.Set<Report>()
                .Any(r => r.UserID == u.Id && 
                r.Year == year &&
                r.WeekNumber == weekNumber))
                .ToListAsync();
        }
    }
    }

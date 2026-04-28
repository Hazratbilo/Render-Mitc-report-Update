using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Interface.Repository;
using MITCRMS.Models.Entities;
using MITCRMS.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MITCRMS.Implementation.Repository
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        public DepartmentRepository(MitcrmsContext mitcrmsContext) : base(mitcrmsContext)
        {
        }

        public async Task<bool> DeleteDepartment(Guid id)
        {
            var department = await _mitcrmsContext.Set<Department>().FindAsync(id);
            if (department == null)
                return false;

            _mitcrmsContext.Set<Department>().Remove(department);
            await _mitcrmsContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByName(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
                return false;

            var name = departmentName.Trim().ToLowerInvariant();
            return await _mitcrmsContext.Set<Department>()
                .AnyAsync(d => d.DepartmentName != null && d.DepartmentName.ToLower() == name);
        }

        public async Task<List<Department>> GetAllDepartments()
        {
            return await _mitcrmsContext.Set<Department>()
                .OrderBy(d => d.DepartmentName)
                .Where(d => d.DepartmentName != "Director of Studies")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Department>> GetAllDepartmentsAndReports()
        {
            return await _mitcrmsContext.Set<Department>()
                .Include(d => d.Users)
                .ThenInclude(d => d.Reports)
                .AsNoTracking()
                .ToListAsync();
        }



        //public async Task<Department> GetDepartmentById(Guid id)
        //{
        //    return await _mitcrmsContext.Set<Department>()
        //        .Include(d => d.Users)
        //        .ThenInclude(d => d.Reports)
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.Id == id);
        //}

        public async Task<Department> UpdateDepartment(Department department)
        {
            _mitcrmsContext.Set<Department>().Update(department);
            await _mitcrmsContext.SaveChangesAsync();
            return department;
        }

        public async Task<Department> GetDepartmentById(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<Department>().FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8603 // Possible null reference return.

        }
        public async Task<bool> DeleteDepartment(Department department)
        {
            _mitcrmsContext.Set<Department>().Remove(department);
            return await _mitcrmsContext.SaveChangesAsync() > 0 ? true : false;

        }
    }
}
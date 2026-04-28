using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using MITCRMS.Contract.Services;
using MITCRMS.Implementation.Repository;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;

using MITCRMS.Models.DTOs.Department;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Entities;

namespace MITCRMS.Implementation.Services
{
    public class DepartmentServices : IDepartmentServices
    {

        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityService _identityService;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DepartmentServices> _logger;
        public DepartmentServices(IUserRepository userRepository,
            UserManager<User> userManager,
            IIdentityService identityService,
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<DepartmentServices> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _identityService = identityService;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResponse<bool>> AddDepartment(CreateDepartmentRequestModel request)
        {
            if (request == null)
            {
                return new BaseResponse<bool>
                {
                    Message = "Fields cannot be empty",
                    Status = false,
                };
            }
            var departmentExists = await _departmentRepository.ExistsByName(request.DepartmentName);
            if (departmentExists)
            {
                _logger.LogError("Department already exists");
                return new BaseResponse<bool>
                {
                    Message = $"Department with name {request.DepartmentName} already exists",
                    Status = false
                };
            }

            var newDepartment = new Department
            {
                DepartmentName = request.DepartmentName,
                DepartmentCode = request.DepartmentCode,
                DateCreated = DateTime.UtcNow
            };
            var createDepartment = await _departmentRepository.Add<Department>(newDepartment);
            await _unitOfWork.SaveChangesAsync();
            if (createDepartment == null)
            {
                _logger.LogError("Create Department Failed");
                return new BaseResponse<bool>
                {
                    Message = "Create Department Failed",
                    Status = false
                };
            }
            _logger.LogInformation("Create Department Success");
            return new BaseResponse<bool>
            {
                Message = "Create Department Success",
                Status = true,

            };
        }
        public async Task<BaseResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync()
        {
            var getAllDepartments = await _departmentRepository.GetAllDepartments();
            if (!getAllDepartments.Any())
            {
                _logger.LogError($"No department found");
                return new BaseResponse<IEnumerable<DepartmentDto>>
                {
                    Message = $"No department found",
                    Status = false,
                };
            }
            _logger.LogInformation("All departments fetched successfully");
            return new BaseResponse<IEnumerable<DepartmentDto>>
            {
                Message = "All departments fetched successfully",
                Status = true,
                Data = getAllDepartments.Select(dpt => new DepartmentDto
                {
                    Id = dpt.Id,
                    DepartmentName = dpt.DepartmentName,
                    DepartmentCode = dpt.DepartmentCode,
                    DateCreated = dpt.DateCreated,
                }).ToList()
            };
        }

        public async Task<IEnumerable<SelectListItem>> GetDepartmentsSelectList()
        {
            var depts = await _departmentRepository.GetAllDepartments();
            return depts.Select(dpt => new SelectListItem
            {
                Text = dpt.DepartmentName,
                Value = dpt.Id.ToString()
            }).ToList();
        }

        public async Task<BaseResponse<DepartmentDto>> GetDepartmentById(Guid id)
        {
            var getNote = await _departmentRepository.GetDepartmentById(id);
            if (getNote == null)
            {
                _logger.LogError($"Department with id {id} not found");
                return new BaseResponse<DepartmentDto>
                {
                    Message = $"Department with id {id} not found",
                    Status = false,
                };
            }
            _logger.LogInformation("Department fetched successfully");
            return new BaseResponse<DepartmentDto>
            {
                Message = "Report fetched successfully",
                Status = true,
                Data = new DepartmentDto
                {
                    Id = getNote.Id,
                    DepartmentName = getNote.DepartmentName,
                    DepartmentCode = getNote.DepartmentCode,
                    DateCreated = getNote.DateCreated,

                }
            };
        }
        public async Task<BaseResponse<bool>> DeleteDepartment(Guid id)
        {
            var getDepartment = await _departmentRepository.GetDepartmentById(id);
            if (getDepartment == null)
            {
                _logger.LogError($"Report with id {id} not found");
                return new BaseResponse<bool>
                {
                    Message = $"Report with id {id} not found",
                    Status = false
                };
            }

            var deleteDepartment = await _departmentRepository.DeleteDepartment(getDepartment);
            if (deleteDepartment)
            {
                _logger.LogInformation("Delete Department Success");
                return new BaseResponse<bool>
                {
                    Message = "Delete Department Success",
                    Status = true
                };
            }
            _logger.LogError("Delete Department Failed");
            return new BaseResponse<bool>
            {
                Message = "Delete Department Failed",
                Status = false
            };
        }
    }
}
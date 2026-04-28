using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Role;

namespace MITCRMS.Implementation.Services
{
    public class RoleService : IRoleServices
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
        {
            _logger = logger;
            _roleRepository = roleRepository;
        }

        public async Task<BaseResponse<IEnumerable<RoleDto>>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetRoles();
            if (!roles.Any())
            {
                _logger.LogError("No roles found");
                return new BaseResponse<IEnumerable<RoleDto>>
                {
                    Message = "No roles found",
                    Status = false
                };
            }

            _logger.LogInformation("Roles fetched successfully");
            return new BaseResponse<IEnumerable<RoleDto>>
            {
                Message = "Roles fetched successfully",
                Status = true,
                Data = roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.RoleName,
                    DateCreated = r.DateCreated,
                }).ToList()
            };
        }
    }
    }
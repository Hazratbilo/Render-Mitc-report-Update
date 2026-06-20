using Microsoft.AspNetCore.Identity;
using Mitc_report_Update.Interface.MailingService;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Role;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Entities;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MITCRMS.Implementation.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository,
            UserManager<User> userManager,
                IDepartmentRepository departmentRepository,
                 IRoleRepository roleRepository,
                 IUnitOfWork unitOfWork,
                 IMailService mailService,
                 IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
       
        public async Task<BaseResponse<bool>> CreateUserAsync(CreateUserRequestModel request)
        {
            if (request == null)
            {
                return new BaseResponse<bool>
                {
                    Message = "All fields are required",
                    Status = false,
                };
            }


            request.Email = request.Email.Trim().ToLowerInvariant();
            request.FirstName = request.FirstName.Trim();
            request.LastName = request.LastName.Trim();
            request.Address = request.Address.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();

            var userExists = await _userRepository.Any(u => u.Email.ToLower() == request.Email);
            if (userExists)
            {
                _logger.LogError("User with email already exist");
                return new BaseResponse<bool>
                {
                    Message = "User with email already exist",
                    Status = false
                };
            }

            var pendingInvite = await _userRepository.Get<StaffInvitation>(i =>
                i.Email.ToLower() == request.Email &&
                i.UsedAt == null &&
                i.ExpiresAt > DateTime.UtcNow);

            if (pendingInvite != null)
            {
                return new BaseResponse<bool>
                {
                    Message = "A valid signup invitation has already been sent to this email",
                    Status = false
                };
            }

            var dept = await _departmentRepository.GetDepartmentById(request.DepartmentId);
            if (dept == null)
            {
                return new BaseResponse<bool>
                {
                    Message = "Department doesn't exist",
                    Status = false,
                };
            }

            try
            {
                if (request.RoleIds == null || !request.RoleIds.Any())
                {
                    return new BaseResponse<bool>
                    {
                        Message = "At least one role must be selected",
                        Status = false
                    };
                }

                var roleIds = await _roleRepository.GetRolesByIdsAsync(r => request.RoleIds.Contains(r.Id));
                var roles = roleIds.Select(r => r.RoleName).ToList();
                if (roles.Count != request.RoleIds.Distinct().Count())
                {
                    return new BaseResponse<bool>
                    {
                        Message = "One or more selected roles cannot be found",
                        Status = false
                    };
                }

                var token = GenerateToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);
                var invite = new StaffInvitation
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    DateCreated = DateTime.UtcNow,
                    DepartmentId = dept.Id,
                    RoleIds = string.Join(",", request.RoleIds.Distinct()),
                    TokenHash = HashToken(token),
                    ExpiresAt = expiresAt
                };

                await _userRepository.Add(invite);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                var mailSent = await _mailService.SendStaffInviteMail(
                    request.Email,
                    $"{request.FirstName} {request.LastName}",
                    BuildSignupLink(token),
                    string.Join(", ", roles),
                    expiresAt);

                if (!mailSent)
                {
                    invite.UsedAt = DateTime.UtcNow;
                    _userRepository.Update(invite);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    _logger.LogError("Unable to send staff invitation email to {Email}", request.Email);
                    return new BaseResponse<bool>
                    {
                        Message = "Unable to send signup invitation email",
                        Status = false
                    };
                }

                _logger.LogInformation("Staff invitation sent successfully");
                return new BaseResponse<bool>
                {
                    Message = "Staff invitation sent successfully",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return new BaseResponse<bool>
                {
                    Message = "Error creating user. Please try again or contact support.",
                    Status = false
                };
            }
        }

        public async Task<BaseResponse<CompleteStaffSignupRequestModel>> GetStaffSignupInviteAsync(string token)
        {
            var invite = await GetValidInviteAsync(token);
            if (invite == null)
            {
                return new BaseResponse<CompleteStaffSignupRequestModel>
                {
                    Message = "Signup link is invalid, expired, or already used",
                    Status = false
                };
            }

            return new BaseResponse<CompleteStaffSignupRequestModel>
            {
                Message = "Signup invite fetched",
                Status = true,
                Data = new CompleteStaffSignupRequestModel
                {
                    Token = token,
                    Email = invite.Email,
                    FullName = $"{invite.FirstName} {invite.LastName}"
                }
            };
        }

        public async Task<BaseResponse<bool>> CompleteStaffSignupAsync(CompleteStaffSignupRequestModel request)
        {
            var invite = await GetValidInviteAsync(request.Token);
            if (invite == null)
            {
                return new BaseResponse<bool>
                {
                    Message = "Signup link is invalid, expired, or already used",
                    Status = false
                };
            }

            var userExists = await _userRepository.Any(u => u.Email.ToLower() == invite.Email.ToLower());
            if (userExists)
            {
                invite.UsedAt = DateTime.UtcNow;
                _userRepository.Update(invite);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                return new BaseResponse<bool>
                {
                    Message = "A staff account already exists for this email",
                    Status = false
                };
            }

            var roleIds = invite.RoleIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToList();

            var selectedRoles = await _roleRepository.GetRolesByIdsAsync(r => roleIds.Contains(r.Id));
            var roles = selectedRoles.Select(r => r.RoleName).ToList();
            if (roles.Count != roleIds.Distinct().Count())
            {
                return new BaseResponse<bool>
                {
                    Message = "One or more invited roles cannot be found",
                    Status = false
                };
            }

            var user = new User
            {
                Email = invite.Email,
                FirstName = invite.FirstName,
                LastName = invite.LastName,
                Address = invite.Address,
                PhoneNumber = invite.PhoneNumber,
                DepartmentId = invite.DepartmentId,
                EmailConfirmed = true,
                DateCreated = DateTime.UtcNow
            };

            var newUser = await _userManager.CreateAsync(user, request.Password);
            if (!newUser.Succeeded)
            {
                var errors = string.Join(" ", newUser.Errors.Select(e => e.Description));
                _logger.LogError("Staff signup unsuccessful: {Errors}", errors);
                return new BaseResponse<bool>
                {
                    Message = string.IsNullOrWhiteSpace(errors) ? "Staff signup unsuccessful" : errors,
                    Status = false
                };
            }

            var roleResult = await _userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Unable to add staff to roles: {Errors}", errors);
                await _userManager.DeleteAsync(user);
                return new BaseResponse<bool>
                {
                    Message = string.IsNullOrWhiteSpace(errors) ? "Unable to add staff to roles" : errors,
                    Status = false
                };
            }

            invite.UsedAt = DateTime.UtcNow;
            _userRepository.Update(invite);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return new BaseResponse<bool>
            {
                Message = "Signup completed successfully. You can now log in.",
                Status = true
            };
        }

        private async Task<StaffInvitation?> GetValidInviteAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var tokenHash = HashToken(token);
            return await _userRepository.Get<StaffInvitation>(i =>
                i.TokenHash == tokenHash &&
                i.UsedAt == null &&
                i.ExpiresAt > DateTime.UtcNow);
        }

        private string BuildSignupLink(string token)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return $"/User/CompleteSignup?token={Uri.EscapeDataString(token)}";
            }

            return $"{request.Scheme}://{request.Host}{request.PathBase}/User/CompleteSignup?token={Uri.EscapeDataString(token)}";
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public async Task<BaseResponse<UserDto>> GetUserProfileByUserId(Guid UserId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserProfile(UserId);
            if (user == null)
            {
                return new BaseResponse<UserDto>
                {
                    Message = "User not found",
                    Status = false
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role == "User")
            {
                return new BaseResponse<UserDto>
                {
                    Message = "User profile fetched",
                    Status = true,
                    Data = new UserDto
                    {

                        Id = user.Id,
                        Email = user.Email,
                        Roles = roles.Select(r => new RoleDto { Name = r }).ToList(),
                       
                            FirstName = user.FirstName,
                            FullName = $"{user.FirstName} {user.LastName}",
                            PhoneNumber = user.PhoneNumber,
                            Address = user.Address,
                    }
                };
            }
            return new BaseResponse<UserDto>
            {
                Message = "SuperAdmin profile fetched",
                Status = true,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = roles.Select(r => new RoleDto { Name = r }).ToList(),
                    }
                };
        }


        public async Task<BaseResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(request.Email);
            if (user == null)
            {
                _logger.LogError("Invalid User");
                return new BaseResponse<LoginResponseModel>
                {
                    Message = "Invalid User",
                    Status = false
                };
            }
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogError("Invalid User");
                return new BaseResponse<LoginResponseModel>
                {
                    Message = "Invalid User",
                    Status = false
                };
            }
            var roles = await _userManager.GetRolesAsync(user);

            var role = roles.FirstOrDefault() ?? string.Empty;

            return new BaseResponse<LoginResponseModel>
            {
                Message = "Login successful",
                Status = true,
                Data = new LoginResponseModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.Select(r => new RoleDto { Name = r }).ToList(),
                    FirstName = user != null ? $"{user.FirstName}" : string.Empty,
                    FullName = user != null ? $"{user.FullName()}" : string.Empty,
                    DepartmentId = user!.DepartmentId,

                }
            };
        }
        //public async Task<List<User>> GetAllUsersWithRolesAsync()
        //{
        //    var user = await _userRepository.GetAllUsersWithRolesAsync();
        //}

        public async Task<List<UserDto>> GetAllUsersWithRolesAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    UserId = user.Id,
                    Email = user.Email,
                    FullName= $"{user.FirstName} {user.LastName}",
                    Departments = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                    {
                        new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Text = user.Department != null ? user.Department.DepartmentName : "No Department",
                            Value = user.DepartmentId.ToString()
                        }
                    },
                    Roles = roles.Select(r => new RoleDto { Name = r }).ToList()
                });
             
            }

            return result;
        }
        public async Task<BaseResponse<UserDto>> GetUserById(Guid id)
        {
            var getUser = await _userRepository.GetUserById(id);
            if (getUser == null)
            {
                _logger.LogError($"User with id {id} not found");
                return new BaseResponse<UserDto>
                {
                    Message = $"User with id {id} not found",
                    Status = false,
                };
            }
            _logger.LogInformation("User fetched successfully");
            return new BaseResponse<UserDto>
            {
                Message = "User fetched successfully",
                Status = true,
                Data = new  UserDto
                {
                    Id = getUser.Id,
                   FirstName= getUser.FirstName,
                    FullName = $"{getUser.FirstName} {getUser.LastName}",
                    LastName = getUser.LastName,
                    Address = getUser.Address,
                    PhoneNumber = getUser.PhoneNumber,
                    Email = getUser.Email,
                        Departments = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                        {
                            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                            {
                                Text = getUser.Department != null ? getUser.Department.DepartmentName : "No Department",
                                Value = getUser.DepartmentId.ToString()
                            }
                        },
                        Roles = (await _userManager.GetRolesAsync(getUser)).Select(r => new RoleDto { Name = r }).ToList(),
                        DateCreated = getUser.DateCreated

                }
            };
        }
        public async Task<BaseResponse<bool>> DeleteUser(Guid id)
        {
            var getUser = await _userRepository.GetUserById(id);
            if (getUser == null)
            {
                _logger.LogError($"User with id {id} not found");
                return new BaseResponse<bool>
                {
                    Message = $"User with id {id} not found",
                    Status = false
                };
            }

            var deleteUser = await _userRepository.DeleteUser(getUser);
            if (deleteUser)
            {
                _logger.LogInformation("Delete User Success");
                return new BaseResponse<bool>
                {
                    Message = "Delete User Success",
                    Status = true
                };
            }
            _logger.LogError("Delete User Failed");
            return new BaseResponse<bool>
            {
                Message = "Delete User Failed",
                Status = false
            };
        }
    }
}

using Microsoft.AspNetCore.Identity;
using MITCRMS.Contract.Services;
using MITCRMS.Identity;
using MITCRMS.Implementation.Repository;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.DTOs.Role;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Entities;

namespace MITCRMS.Implementation.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IIdentityService _identityService;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository,
            UserManager<User> userManager,
                IIdentityService identityService,
                IDepartmentRepository departmentRepository,
                 IUnitOfWork unitOfWork, IRoleRepository roleRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _identityService = identityService;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
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


            var userExists = await _userRepository.Any(u => u.Email == request.Email);
            if (userExists)
            {
                _logger.LogError("User with email already exist");
                return new BaseResponse<bool>
                {
                    Message = "User with email already exist",
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
                var User = new User
                {
                    Email = request.Email,
                    PasswordHash = _identityService.GetPasswordHash(request.PasswordHash),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    DateCreated = DateTime.UtcNow,
                    DepartmentId = dept.Id,
                    

                };

                var newUser = await _userManager.CreateAsync(User);
                if (newUser == null)
                {
                    _logger.LogError("User Creation unsuccessful");
                    return new BaseResponse<bool>
                    {
                        Message = "User Creation unsuccessful",
                        Status = false
                    };

                }

                var roleIds = await _roleRepository.GetRolesByIdsAsync(r => request.RoleIds.Contains(r.Id));
                var roles = roleIds.Select(r => r.RoleName).ToList();

                if(roles is null || !roles.Any())
                {
                    return new BaseResponse<bool>
                    {
                        Message = "Roles cannot be found"
                    };
                }

                var result = await _userManager.AddToRolesAsync(User, roles);
                if (!result.Succeeded)
                {
                    _logger.LogError("Unable to add user to roles");
                    return new BaseResponse<bool>
                    {
                        Message = "Unable to add user to roles",
                        Status = false
                    };
                }

                _logger.LogInformation("User added successfully");
                return new BaseResponse<bool>
                {
                    Message = "User added successfully",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error  creating User, rolling back.....");
                return new BaseResponse<bool>
                {
                    Message = "Error  creating User, rolling back.....",
                    Status = false
                };
            }
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
                    Roles = role.Select(r => new RoleDto { Name = role }).ToList(),
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
                    Roles = role.Select(r => new RoleDto { Name = role }).ToList(),
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
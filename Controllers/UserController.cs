using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MITCRMS.Implementation.Services;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Entities;
using System.Security.Claims;

namespace MITCRMS.Controllers
{
    public class UserController(IDepartmentServices departmentServices,
        IRoleServices roleServices, ILogger<UserController> logger, IUserServices userServices,
        IReportServices reportServices) : Controller
    {
        private readonly IRoleServices _roleServices = roleServices ?? throw new ArgumentNullException(nameof(roleServices));
        private readonly ILogger<UserController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUserServices _userService = userServices ?? throw new ArgumentNullException(nameof(userServices));
        private readonly IReportServices _reportServices = reportServices ?? throw new ArgumentNullException(nameof(reportServices));
        private readonly IDepartmentServices _departmentServices = departmentServices ?? throw new ArgumentNullException(nameof(departmentServices));
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles="Hod,Tutor,Bursar,Admin")]
        public  async Task<IActionResult> StaffDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("Invalid user ID");
            }

            var resp = await _reportServices.GetMyReportsAsync(userId);

            var reportsCount = resp.Data?.Count() ?? 0;
            
            ViewBag.ReportsCount = reportsCount;
            
            return View(resp.Data); 
        }
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuperAdminDashboard()
        {
            var reports = await _reportServices.GetAllReportsAsync();

            return View(reports ?? new List<ReportDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            var loginResponse = await _userService.LoginAsync(model, CancellationToken.None);
            var checkRole = "";
            if (loginResponse.Status)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginResponse.Data.FirstName),
                    new Claim(ClaimTypes.GivenName, loginResponse.Data.FullName),
                    new Claim(ClaimTypes.Email, loginResponse.Data.Email),
                    new Claim(ClaimTypes.NameIdentifier, loginResponse.Data.UserId.ToString()),
                    new Claim("DepartmentId", loginResponse.Data.DepartmentId.ToString())
                };

                foreach (var role in loginResponse.Data.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    checkRole = role.Name;
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authenticationProperties = new AuthenticationProperties();
                var principal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authenticationProperties);
               
                
                if (checkRole == "SuperAdmin")
                {
                    return RedirectToAction("SuperAdminDashboard", "User");
                }

                return RedirectToAction("StaffDashboard", "User");

            }
            else
            {
                ViewBag.ErrorMessage = loginResponse.Message;
                return View(model);
            }

        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserId");

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Login", "User");
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleServices.GetRolesAsync();
            var departments = await _departmentServices.GetAllDepartmentsAsync();

            if (departments?.Data == null || !departments.Data.Any())
            {
                return RedirectToAction("CreateDepartment", "Department");
            }

            ViewData["Departments"] = new SelectList(departments.Data, "Id", "DepartmentName");
            ViewData["Roles"] = new MultiSelectList(roles.Data, "Id", "Name");

            return View();
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateUser(CreateUserRequestModel model)
        {
            var roles = await _roleServices.GetRolesAsync();
            var departments = await _departmentServices.GetAllDepartmentsAsync();

            if (departments?.Data == null || !departments.Data.Any())
            {
                return RedirectToAction("CreateDepartment", "Department");
            }

            ViewData["Departments"] = new SelectList(departments.Data, "Id", "DepartmentName", model.DepartmentId);
            ViewData["Roles"] = new MultiSelectList(roles.Data, "Id", "Name", model.RoleIds);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userStatus = await _userService.CreateUserAsync(model);
            if (userStatus.Status)
            {
                ViewBag.Alert = userStatus.Status;
                ViewBag.AlertType = "success";

                return RedirectToAction("GetAllUsersWithRoles");
            }
            else
            {

                ViewBag.Alert = userStatus.Status;
                ViewBag.AlertType = "danger";
                ModelState.AddModelError("Email", userStatus.Message);
                
            }
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Userprofile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "User");
            }
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var user = await _userService.GetUserProfileByUserId(userId, CancellationToken.None);

#pragma warning disable CS8602
            if (user == null || !user.Status) return NotFound(user.Message);
#pragma warning restore CS8602 

            return View(user);
        }
        [HttpGet]
        [Authorize(Roles = "Hod,Tutor,Bursar,Admin")]
        public async Task<IActionResult> GetMyReport()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("UserProfile", "User");
            }
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID format.");
            }
            var reports = await _reportServices.GetReportByIdAsync(userId, CancellationToken.None);

            return View(reports);
        }

      [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllUsersWithRolesAsync(string search)
    {
            ViewBag.SearchTerm = search;

            var users = await _userService.GetAllUsersWithRolesAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search)
                ).ToList();
            }

        return View(users);
    }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var dept = await _userService.GetUserById(id);
            if (dept == null || !dept.Status) return NotFound();

            return View("DeleteUser", dept);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(Guid id)
        {
            await _userService.DeleteUser(id);
            return RedirectToAction("GetAllUsersWithRoles");
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var response = await _userService.GetUserById(id);

            if (response == null)
            {
                return NotFound();
            }

            if (response.Data == null)
            {
                return NotFound(response.Message ?? "User not found.");
            }

            return View(response.Data);
        }
    }
}

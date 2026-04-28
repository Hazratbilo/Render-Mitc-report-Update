using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MITCRMS.Implementation.Services;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs.Department;

namespace MITCRMS.Controllers
{
    public class DepartmentController : Controller
    {
        private IDepartmentServices _departmentServices;

        public DepartmentController(IDepartmentServices departmentServices)
        {
            _departmentServices = departmentServices;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _departmentServices.GetAllDepartmentsAsync();
            if (response == null || !response.Status || response.Data == null)
            {
                ViewBag.Message = response?.Message ?? "No data found";
                return View(Array.Empty<DepartmentDto>());
            }

            return View(response.Data);
        }
        [HttpGet]
        public IActionResult CreateDepartment()
        {
            return View();
        }





        [HttpPost]
        [Authorize(Roles ="SuperAdmin")]
        public async Task<IActionResult> CreateDepartment(CreateDepartmentRequestModel model)
        {
            var result = await _departmentServices.AddDepartment(model);
            if (!result.Status)
            {
            //    ModelState.AddModelError(string.Empty, result.Message ?? "Could not create department");
                return View(model);
            }

            return RedirectToAction(nameof(GetAllDepartment));
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDepartment([FromRoute] Guid id)
        {
            var response = await _departmentServices.GetDepartmentById(id);
            if (response == null || !response.Status || response.Data == null)
            {
                return NotFound(response?.Message ?? "Department not found");
            }

            return View(response.Data);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var response = await _departmentServices.GetDepartmentById(id);
            if (response == null || !response.Status || response.Data == null)
            {
                return NotFound(response?.Message ?? "Department not found");
            }

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var deleteResponse = await _departmentServices.DeleteDepartment(id);
            if (deleteResponse == null || !deleteResponse.Status)
            {
                TempData["Error"] = deleteResponse?.Message ?? "Delete failed";
            }

            return RedirectToAction(nameof(GetAllDepartment));
        }
        [Authorize(Roles ="SuperAdmin")]
        public async Task<IActionResult> GetAllDepartment()
        {
            var departments = await _departmentServices.GetAllDepartmentsAsync();
            return View(departments.Data);
        }
    }
}
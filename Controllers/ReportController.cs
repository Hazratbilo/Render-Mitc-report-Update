using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Contract.Services;
using MITCRMS.Implementation.Services;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.Entities;
using MITCRMS.Models.Enum;
using System.Security.Claims;

namespace MITCRMS.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportServices _reportServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly IIdentityService _identityService;
        private readonly ILogger<ReportController> _logger;
        private readonly IWebHostEnvironment _env;

#pragma warning disable CS8618 
        public ReportController(
#pragma warning restore CS8618
            IReportServices reportServices,
            IDepartmentServices departmentServices,
            IIdentityService identityService, IWebHostEnvironment env,
            ILogger<ReportController> logger)
        {
            _reportServices = reportServices;
            _departmentServices = departmentServices;
            _env = env;
            _identityService = identityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentUser = await _identityService.GetLoggedInUser();
            var roles = await _identityService.GetRolesAsync(currentUser);

            if (roles.Contains("User"))
            {
                var hodId = currentUser.Id;
                var hodall = await _reportServices.GetUserReportByIdAsync(hodId, cancellationToken);

                var hodReports = (hodall.Data ?? Enumerable.Empty<ReportDto>()).Where(r => r.Id == hodId);
                return View(hodReports);

            }
           
            else
            {
                return View(null);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Tutor,Hod,Bursar,Admin")]
        public async Task<IActionResult> CreateReport()
        {
            var depts = await _departmentServices.GetDepartmentsSelectList();
            ViewData["Departments"] = new SelectList(depts, "Value", "Text");
            return View(new CreateReportRequestModel());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tutor,Hod,Bursar,Admin")]
        public async Task<IActionResult> CreateReport([FromForm]IFormFile? file, CreateReportRequestModel model)
        {


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
            {
                TempData["Error"] = "User information not found. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var role = roleClaim.Value;


            var fileUrl = "";

            if (file != null && file.Length > 0)
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsPath = Path.Combine(webRoot, "reports");

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }


                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension.Replace(",", ".")}";
                var fullPath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

            }
            var result = await _reportServices.CreateReportAsync(fileUrl, model, userId, role);

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            TempData["Success"] = "Report created successfully!";
            return RedirectToAction("GetMyReports", "Report");
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var resp = await _reportServices.AcceptReport(id);
            if (!resp.Status)
            {
                TempData["Error"] = resp.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
      
        [HttpGet]
        [Authorize(Roles = "Tutor,Hod,Bursar,Admin")]
        public async Task<IActionResult> GetMyReports(string? status, string? search)
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
            var reports = resp.Data ?? new List<ReportDto>();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReportStatus>(status, true, out var parsedStatus))
            {
                reports = reports.Where(r => r.Status == parsedStatus).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                reports = reports.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.Tittle) && r.Tittle.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Content) && r.Content.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.DepartmentName) && r.DepartmentName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = search;

            return View(reports);
        }
        [HttpGet]
        [Authorize(Roles = "Hod")]
        public async Task<IActionResult>GetReportsByDepartmentId(string? status, string? search)
        {
            var departmentIdClaim = User.FindFirst("DepartmentId")?.Value;

            Console.WriteLine("DepartmentId Claim: " + departmentIdClaim);

            if (!Guid.TryParse(departmentIdClaim, out var deptartmentId))
            {
                return BadRequest("Invalid department ID");
            }
            
            Console.WriteLine("DepartmentId: " + departmentIdClaim);
            var resp = await _reportServices.GetAllReportsByDepartmentIdAsync(deptartmentId, CancellationToken.None);
            if (!resp.Status || resp.Data == null)
                return NotFound(resp.Message);

            var reports = resp.Data;

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReportStatus>(status, true, out var parsedStatus))
            {
                reports = reports.Where(r => r.Status == parsedStatus).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                reports = reports.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.Tittle) && r.Tittle.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Content) && r.Content.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (r.User != null && !string.IsNullOrWhiteSpace(r.User.FullName) && r.User.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = search;

            return View(reports);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var dept = await _reportServices.GetReportById(id);
            if (dept == null || !dept.Status) return NotFound();

            return View("DeleteReport",dept);
        }

        [HttpPost, ActionName("DeleteReport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReportConfirmed(Guid id)
        {
            await _reportServices.DeleteReport(id);
            return RedirectToAction("GetMyReports");
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var department = await _reportServices.GetReportById(id);
            if (department == null)
            {
                throw new Exception("Request not found!");
            }

            return View(department);
        }
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            var report = await _reportServices.GetReportById(id);
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph(report.Data.Tittle).SetFontSize(18));
            document.Add(new Paragraph(report.Data.Content));


            //var textToAdd = report.Data.FileUrl ?? string.Empty;
            //document.Add(new Paragraph(textToAdd));

            if (!string.IsNullOrEmpty(report.Data.FileUrl))
            {
                var filePath = report.Data.FileUrl;
                var extension = System.IO.Path.GetExtension(filePath).ToLower();

                try
                {
                    if(extension == ".pdf")
                    {
                        using var sourceReader = new PdfReader(filePath);
                        using var sourcePdf = new PdfDocument(sourceReader);
                        sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), pdf);
                    }
                    else if(new[] { ".jpg", ".png", ".jpeg" }.Contains(extension))
                    {
                        var imageData = ImageDataFactory.Create(filePath);
                        document.Add(new Image(imageData).SetMaxHeight(400).SetHorizontalAlignment(HorizontalAlignment.CENTER));
                    }
                    else
                    {
                        var link = new Link("Click here to view attachment",
                            PdfAction.CreateURI(report.Data.FileUrl)).SetFontColor(ColorConstants.BLUE);
                        document.Add(new Paragraph().Add(link));
                    }
                }
                catch(Exception ex)
                {
                    document.Add(new Paragraph($"[Error loading attachment: {ex.Message}]")).SetFontColor(ColorConstants.RED);
                }
            }

            document.Add(new Paragraph(report.Data.DateCreated.ToString("yyyy-MM-dd")));

            document.Close();

            var fileName = SanitizeFileName(report.Data.Tittle) + ".pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }
        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "note";
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                input = input.Replace(c, '-');
            return input;
        }
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllReports(string? status, string? search)
        {
            var reports = await _reportServices.GetAllReportsAsync();
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ReportStatus>(status, out var parsedStatus))
                {
                    reports = reports
                        .Where(r => r.Status == parsedStatus)
                        .ToList();
                }
                else
                {
                    reports = new List<ReportDto>();
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                reports = reports.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.Tittle) && r.Tittle.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.FullName) && r.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.DepartmentName) && r.DepartmentName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Content) && r.Content.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = search;

            return View(reports);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
      public async Task<IActionResult> ApproveReport(Guid id)
{
    var success = await _reportServices.ChangeReportStatusAsync(id, ReportStatus.Approved);

    if (success)
        TempData["SuccessMessage"] = "Report Approved!";
    else
        TempData["ErrorMessage"] = "Unable to process approval.";

    return RedirectToAction(nameof(GetAllReports));
}

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CancelReport(Guid id)
        {

            var success = await _reportServices.ChangeReportStatusAsync(id, ReportStatus.Rejected);

            if (success)
                TempData["SuccessMessage"] = "Report Rejected!";
            else
                TempData["ErrorMessage"] = "Unable to process rejectection.";

            return RedirectToAction(nameof(GetAllReports));
        }
    }



}


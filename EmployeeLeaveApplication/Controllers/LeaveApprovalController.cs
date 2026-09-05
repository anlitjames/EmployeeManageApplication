using System.Security.Claims;
using EmployeeLeaveApplication.Services;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveApplication.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class LeaveApprovalController : Controller
    {
        private readonly ILeaveApprovalService _approvalService;
        private readonly ILogger<LeaveApprovalController> _logger;

        public LeaveApprovalController(
            ILeaveApprovalService approvalService,
            ILogger<LeaveApprovalController> logger)
        {
            _approvalService = approvalService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            int? managerEmpId = null;

            var empIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(empIdClaim, out var parsedEmpId) && parsedEmpId > 0)
            {
                managerEmpId = parsedEmpId;
            }

            var model = await _approvalService.GetPendingApplicationsAsync(userId, role, managerEmpId);
            model.CurrentUserName = User.Identity?.Name ?? string.Empty;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            int? managerEmpId = null;

            var empIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(empIdClaim, out var parsedEmpId) && parsedEmpId > 0)
            {
                managerEmpId = parsedEmpId;
            }

            var result = await _approvalService.ApproveApplicationAsync(id, userId, role, managerEmpId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = $"Leave Application #{id} approved successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(RejectLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Rejection remarks are mandatory.";
                return RedirectToAction(nameof(Index));
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            int? managerEmpId = null;

            var empIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(empIdClaim, out var parsedEmpId) && parsedEmpId > 0)
            {
                managerEmpId = parsedEmpId;
            }

            var result = await _approvalService.RejectApplicationAsync(model.ApplicationId, model.RejectionRemarks, userId, role, managerEmpId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = $"Leave Application #{model.ApplicationId} was rejected.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

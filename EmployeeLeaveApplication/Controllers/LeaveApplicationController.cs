using EmployeeLeaveApplication.Services;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveApplication.Controllers
{
    [Authorize]
    public class LeaveApplicationController : Controller
    {
        private readonly ILeaveApplicationService _leaveAppService;
        private readonly ILogger<LeaveApplicationController> _logger;

        public LeaveApplicationController(
            ILeaveApplicationService leaveAppService,
            ILogger<LeaveApplicationController> logger)
        {
            _leaveAppService = leaveAppService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? employeeId, int? leaveTypeId, string? status, int? year)
        {
            var model = await _leaveAppService.GetApplicationsAsync(employeeId, leaveTypeId, status, year);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var model = new LeaveApplicationCreateViewModel
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                NumberOfDays = 1,
                EmployeeList = await _leaveAppService.GetActiveEmployeesSelectListAsync(),
                LeaveTypeList = await _leaveAppService.GetActiveLeaveTypesSelectListAsync()
            };

            // Pre-select employee if linked to logged-in user
            var empIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(empIdClaim, out var currentEmpId) && currentEmpId > 0)
            {
                model.EmployeeId = currentEmpId;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveApplicationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.EmployeeList = await _leaveAppService.GetActiveEmployeesSelectListAsync();
                model.LeaveTypeList = await _leaveAppService.GetActiveLeaveTypesSelectListAsync();
                return View(model);
            }

            var result = await _leaveAppService.ApplyAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to apply for leave.");
                model.EmployeeList = await _leaveAppService.GetActiveEmployeesSelectListAsync();
                model.LeaveTypeList = await _leaveAppService.GetActiveLeaveTypesSelectListAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "Leave application submitted successfully with status: Pending.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance(int employeeId, int leaveTypeId, int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var balance = await _leaveAppService.GetEmployeeLeaveBalanceAsync(employeeId, leaveTypeId, targetYear);
            return Json(new
            {
                allocated = balance.Allocated,
                used = balance.Used,
                available = balance.Available
            });
        }

        [HttpGet]
        public IActionResult CalculateDays(DateTime fromDate, DateTime toDate)
        {
            var days = _leaveAppService.CalculateDays(fromDate, toDate);
            return Json(new { days = days });
        }
    }
}

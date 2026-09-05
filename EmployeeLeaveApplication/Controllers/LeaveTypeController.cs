using EmployeeLeaveApplication.Services;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LeaveTypeController : Controller
    {
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILogger<LeaveTypeController> _logger;

        public LeaveTypeController(ILeaveTypeService leaveTypeService, ILogger<LeaveTypeController> logger)
        {
            _leaveTypeService = leaveTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _leaveTypeService.GetAllAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LeaveTypeCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveTypeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _leaveTypeService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create leave type.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Leave Type '{model.LeaveTypeName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _leaveTypeService.GetForEditAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LeaveTypeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _leaveTypeService.UpdateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update leave type.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Leave Type '{model.LeaveTypeName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _leaveTypeService.ToggleStatusAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Leave type status updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

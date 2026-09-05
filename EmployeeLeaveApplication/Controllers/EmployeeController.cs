using EmployeeLeaveApplication.Services;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? departmentId,
            string? statusFilter,
            int pageNumber = 1)
        {
            const int pageSize = 10;
            var model = await _employeeService.GetEmployeesAsync(searchTerm, departmentId, statusFilter, pageNumber, pageSize);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _employeeService.GetDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new EmployeeCreateViewModel
            {
                DepartmentList = await _employeeService.GetDepartmentSelectListAsync(),
                ManagerList = await _employeeService.GetManagerSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DepartmentList = await _employeeService.GetDepartmentSelectListAsync();
                model.ManagerList = await _employeeService.GetManagerSelectListAsync();
                return View(model);
            }

            var result = await _employeeService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create employee.");
                model.DepartmentList = await _employeeService.GetDepartmentSelectListAsync();
                model.ManagerList = await _employeeService.GetManagerSelectListAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = $"Employee {model.EmployeeName} ({model.EmployeeCode}) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _employeeService.GetForEditAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DepartmentList = await _employeeService.GetDepartmentSelectListAsync();
                model.ManagerList = await _employeeService.GetManagerSelectListAsync(model.Id);
                return View(model);
            }

            var result = await _employeeService.UpdateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update employee.");
                model.DepartmentList = await _employeeService.GetDepartmentSelectListAsync();
                model.ManagerList = await _employeeService.GetManagerSelectListAsync(model.Id);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Employee {model.EmployeeName} ({model.EmployeeCode}) updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _employeeService.ToggleStatusAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Employee status updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEmployeeCode(string employeeCode, int? id)
        {
            var isUnique = await _employeeService.IsEmployeeCodeUniqueAsync(employeeCode, id);
            return Json(isUnique ? (object)true : $"Employee Code '{employeeCode}' is already taken.");
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEmail(string email, int? id)
        {
            var isUnique = await _employeeService.IsEmailUniqueAsync(email, id);
            return Json(isUnique ? (object)true : $"Email '{email}' is already registered.");
        }
    }
}

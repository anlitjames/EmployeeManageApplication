using EmployeeLeaveApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveApplication.Controllers
{
    [Authorize]
    public class LeaveBalanceController : Controller
    {
        private readonly ILeaveBalanceService _balanceService;

        public LeaveBalanceController(ILeaveBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        public async Task<IActionResult> Index(int? employeeId, int? leaveTypeId, int? year)
        {
            var model = await _balanceService.GetLeaveBalancesAsync(employeeId, leaveTypeId, year);
            return View(model);
        }
    }
}

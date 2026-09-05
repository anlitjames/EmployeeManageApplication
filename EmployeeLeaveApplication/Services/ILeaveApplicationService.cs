using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveApplication.Services
{
    public interface ILeaveApplicationService
    {
        Task<LeaveBalanceSummaryDto> GetEmployeeLeaveBalanceAsync(int employeeId, int leaveTypeId, int year);
        Task<List<LeaveBalanceSummaryDto>> GetAllBalancesForEmployeeAsync(int employeeId, int year);
        Task<(bool Success, string? ErrorMessage)> ApplyAsync(LeaveApplicationCreateViewModel model);
        Task<LeaveApplicationListViewModel> GetApplicationsAsync(int? employeeId, int? leaveTypeId, string? status, int? year);
        Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime fromDate, DateTime toDate, int? excludeApplicationId = null);
        decimal CalculateDays(DateTime fromDate, DateTime toDate);
        Task<List<SelectListItem>> GetActiveEmployeesSelectListAsync();
        Task<List<SelectListItem>> GetActiveLeaveTypesSelectListAsync();
    }
}

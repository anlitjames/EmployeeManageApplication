using EmployeeLeaveApplication.ViewModels;

namespace EmployeeLeaveApplication.Services
{
    public interface ILeaveBalanceService
    {
        Task<LeaveBalanceListViewModel> GetLeaveBalancesAsync(int? employeeId, int? leaveTypeId, int? year);
    }
}

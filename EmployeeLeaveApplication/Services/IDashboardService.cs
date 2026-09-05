using EmployeeLeaveApplication.ViewModels;

namespace EmployeeLeaveApplication.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardMetricsAsync();
    }
}

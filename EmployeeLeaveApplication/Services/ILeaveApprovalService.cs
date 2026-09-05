using EmployeeLeaveApplication.ViewModels;

namespace EmployeeLeaveApplication.Services
{
    public interface ILeaveApprovalService
    {
        Task<LeaveApprovalListViewModel> GetPendingApplicationsAsync(int userId, string role, int? managerEmployeeId);
        Task<(bool Success, string? ErrorMessage)> ApproveApplicationAsync(int applicationId, int approvedByUserId, string role, int? managerEmployeeId);
        Task<(bool Success, string? ErrorMessage)> RejectApplicationAsync(int applicationId, string rejectionRemarks, int approvedByUserId, string role, int? managerEmployeeId);
        Task EnsureConcurrencySafeStoredProcedureAsync();
    }
}

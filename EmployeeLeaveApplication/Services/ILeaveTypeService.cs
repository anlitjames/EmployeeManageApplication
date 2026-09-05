using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;

namespace EmployeeLeaveApplication.Services
{
    public interface ILeaveTypeService
    {
        Task<List<LeaveTypeItemViewModel>> GetAllAsync();
        Task<LeaveType?> GetByIdAsync(int id);
        Task<LeaveTypeEditViewModel?> GetForEditAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(LeaveTypeCreateViewModel model);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(LeaveTypeEditViewModel model);
        Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}

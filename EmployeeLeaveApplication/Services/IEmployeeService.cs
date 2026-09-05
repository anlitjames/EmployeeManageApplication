using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveApplication.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeListViewModel> GetEmployeesAsync(string? searchTerm, int? departmentId, string? statusFilter, int pageNumber, int pageSize);
        Task<Employee?> GetByIdAsync(int id);
        Task<EmployeeDetailsViewModel?> GetDetailsAsync(int id);
        Task<EmployeeEditViewModel?> GetForEditAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(EmployeeCreateViewModel model);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(EmployeeEditViewModel model);
        Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int id);
        Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<List<SelectListItem>> GetDepartmentSelectListAsync();
        Task<List<SelectListItem>> GetManagerSelectListAsync(int? excludeEmployeeId = null);
    }
}

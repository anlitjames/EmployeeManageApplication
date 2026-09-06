using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmployeeListViewModel> GetEmployeesAsync(
            string? searchTerm,
            int? departmentId,
            string? statusFilter,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.ReportingManager)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(e =>
                    e.EmployeeName.ToLower().Contains(term) ||
                    e.EmployeeCode.ToLower().Contains(term) ||
                    e.Email.ToLower().Contains(term));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.IsActive);
                }
                else if (statusFilter.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => !e.IsActive);
                }
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EmployeeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeItemViewModel
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    Email = e.Email,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
                    DateOfJoining = e.DateOfJoining,
                    ReportingManagerName = e.ReportingManager != null ? e.ReportingManager.EmployeeName : null,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            var departmentList = await GetDepartmentSelectListAsync();

            return new EmployeeListViewModel
            {
                Employees = items,
                SearchTerm = searchTerm,
                DepartmentId = departmentId,
                StatusFilter = statusFilter ?? "all",
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                DepartmentList = departmentList
            };
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.ReportingManager)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EmployeeDetailsViewModel?> GetDetailsAsync(int id)
        {
            var emp = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.ReportingManager)
                .Include(e => e.Subordinates)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emp == null) return null;

            return new EmployeeDetailsViewModel
            {
                Id = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                EmployeeName = emp.EmployeeName,
                Email = emp.Email,
                DepartmentName = emp.Department?.DepartmentName ?? "N/A",
                DateOfJoining = emp.DateOfJoining,
                ReportingManagerName = emp.ReportingManager?.EmployeeName,
                IsActive = emp.IsActive,
                CreatedDate = emp.CreatedDate,
                ModifiedDate = emp.ModifiedDate,
                Subordinates = emp.Subordinates.Select(s => $"{s.EmployeeCode} - {s.EmployeeName}").ToList()
            };
        }

        public async Task<EmployeeEditViewModel?> GetForEditAsync(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return null;

            return new EmployeeEditViewModel
            {
                Id = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                EmployeeName = emp.EmployeeName,
                Email = emp.Email,
                DepartmentId = emp.DepartmentId,
                DateOfJoining = emp.DateOfJoining,
                ReportingManagerId = emp.ReportingManagerId,
                IsActive = emp.IsActive,
                DepartmentList = await GetDepartmentSelectListAsync(),
                ManagerList = await GetManagerSelectListAsync(id)
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(EmployeeCreateViewModel model)
        {
            if (!await IsEmployeeCodeUniqueAsync(model.EmployeeCode))
            {
                return (false, $"Employee Code '{model.EmployeeCode}' is already in use.");
            }

            if (!await IsEmailUniqueAsync(model.Email))
            {
                return (false, $"Email '{model.Email}' is already registered.");
            }

            var employee = new Employee
            {
                EmployeeCode = model.EmployeeCode.Trim().ToUpper(),
                EmployeeName = model.EmployeeName.Trim(),
                Email = model.Email.Trim().ToLower(),
                DepartmentId = model.DepartmentId,
                DateOfJoining = model.DateOfJoining,
                ReportingManagerId = model.ReportingManagerId,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeCode} created successfully.", employee.EmployeeCode);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(EmployeeEditViewModel model)
        {
            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null)
            {
                return (false, "Employee not found.");
            }

            if (model.ReportingManagerId.HasValue && model.ReportingManagerId.Value == model.Id)
            {
                return (false, "An employee cannot be their own reporting manager.");
            }

            if (!await IsEmployeeCodeUniqueAsync(model.EmployeeCode, model.Id))
            {
                return (false, $"Employee Code '{model.EmployeeCode}' is already in use.");
            }

            if (!await IsEmailUniqueAsync(model.Email, model.Id))
            {
                return (false, $"Email '{model.Email}' is already registered.");
            }

            var trimmedName = model.EmployeeName.Trim();

            // Check if there is a linked User account for this Employee
            var linkedUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == model.Id);
            if (linkedUser != null && !string.Equals(linkedUser.Username, trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                // Validate that the new username is not already taken by another user
                var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == trimmedName.ToLower() && u.Id != linkedUser.Id);
                if (usernameExists)
                {
                    return (false, $"Cannot update employee: A user account with the username '{trimmedName}' already exists.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                employee.EmployeeCode = model.EmployeeCode.Trim().ToUpper();
                employee.EmployeeName = trimmedName;
                employee.Email = model.Email.Trim().ToLower();
                employee.DepartmentId = model.DepartmentId;
                employee.DateOfJoining = model.DateOfJoining;
                employee.ReportingManagerId = model.ReportingManagerId;
                employee.IsActive = model.IsActive;
                employee.ModifiedDate = DateTime.Now;

                // Synchronize Username to match EmployeeName
                if (linkedUser != null)
                {
                    linkedUser.Username = trimmedName;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Employee {EmployeeCode} updated successfully (Synchronized User: {HasUser}).", 
                    employee.EmployeeCode, linkedUser != null);

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update employee {EmployeeCode} and synchronize user.", employee.EmployeeCode);
                return (false, "An unexpected error occurred while saving employee changes.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return (false, "Employee not found.");
            }

            employee.IsActive = !employee.IsActive;
            employee.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeCode} status changed to {Status}.", employee.EmployeeCode, employee.IsActive ? "Active" : "Inactive");
            return (true, null);
        }

        public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, int? excludeId = null)
        {
            var normalized = employeeCode.Trim().ToUpper();
            var query = _context.Employees.Where(e => e.EmployeeCode.ToUpper() == normalized);
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            var normalized = email.Trim().ToLower();
            var query = _context.Employees.Where(e => e.Email.ToLower() == normalized);
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<List<SelectListItem>> GetDepartmentSelectListAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.DepartmentName
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetManagerSelectListAsync(int? excludeEmployeeId = null)
        {
            var query = _context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive);

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEmployeeId.Value);
            }

            return await query
                .OrderBy(e => e.EmployeeName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.EmployeeName} ({e.EmployeeCode})"
                })
                .ToListAsync();
        }
    }
}

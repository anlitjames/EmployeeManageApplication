using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveTypeService> _logger;

        public LeaveTypeService(ApplicationDbContext context, ILogger<LeaveTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LeaveTypeItemViewModel>> GetAllAsync()
        {
            return await _context.LeaveTypes
                .AsNoTracking()
                .OrderBy(lt => lt.LeaveTypeName)
                .Select(lt => new LeaveTypeItemViewModel
                {
                    Id = lt.Id,
                    LeaveTypeName = lt.LeaveTypeName,
                    Description = lt.Description,
                    AnnualAllocation = lt.AnnualAllocation,
                    CarryForwardAllowed = lt.CarryForwardAllowed,
                    MaxCarryForwardDays = lt.MaxCarryForwardDays,
                    IsActive = lt.IsActive,
                    ApplicationsCount = lt.LeaveApplications.Count
                })
                .ToListAsync();
        }

        public async Task<LeaveType?> GetByIdAsync(int id)
        {
            return await _context.LeaveTypes.FindAsync(id);
        }

        public async Task<LeaveTypeEditViewModel?> GetForEditAsync(int id)
        {
            var lt = await _context.LeaveTypes.FindAsync(id);
            if (lt == null) return null;

            return new LeaveTypeEditViewModel
            {
                Id = lt.Id,
                LeaveTypeName = lt.LeaveTypeName,
                Description = lt.Description,
                AnnualAllocation = lt.AnnualAllocation,
                CarryForwardAllowed = lt.CarryForwardAllowed,
                MaxCarryForwardDays = lt.MaxCarryForwardDays,
                IsActive = lt.IsActive
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(LeaveTypeCreateViewModel model)
        {
            if (!await IsNameUniqueAsync(model.LeaveTypeName))
            {
                return (false, $"Leave Type '{model.LeaveTypeName}' already exists.");
            }

            // Enforce business rule: If CarryForwardAllowed = false, MaxCarryForwardDays MUST be 0
            var maxCarryForward = model.CarryForwardAllowed ? model.MaxCarryForwardDays : 0;

            var leaveType = new LeaveType
            {
                LeaveTypeName = model.LeaveTypeName.Trim(),
                Description = model.Description?.Trim(),
                AnnualAllocation = model.AnnualAllocation,
                CarryForwardAllowed = model.CarryForwardAllowed,
                MaxCarryForwardDays = maxCarryForward,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now
            };

            _context.LeaveTypes.Add(leaveType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave Type '{Name}' created.", leaveType.LeaveTypeName);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(LeaveTypeEditViewModel model)
        {
            var leaveType = await _context.LeaveTypes.FindAsync(model.Id);
            if (leaveType == null)
            {
                return (false, "Leave Type not found.");
            }

            if (!await IsNameUniqueAsync(model.LeaveTypeName, model.Id))
            {
                return (false, $"Leave Type '{model.LeaveTypeName}' already exists.");
            }

            // Enforce business rule: If CarryForwardAllowed = false, MaxCarryForwardDays MUST be 0
            var maxCarryForward = model.CarryForwardAllowed ? model.MaxCarryForwardDays : 0;

            leaveType.LeaveTypeName = model.LeaveTypeName.Trim();
            leaveType.Description = model.Description?.Trim();
            leaveType.AnnualAllocation = model.AnnualAllocation;
            leaveType.CarryForwardAllowed = model.CarryForwardAllowed;
            leaveType.MaxCarryForwardDays = maxCarryForward;
            leaveType.IsActive = model.IsActive;
            leaveType.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave Type '{Name}' updated.", leaveType.LeaveTypeName);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> ToggleStatusAsync(int id)
        {
            var leaveType = await _context.LeaveTypes.FindAsync(id);
            if (leaveType == null)
            {
                return (false, "Leave Type not found.");
            }

            leaveType.IsActive = !leaveType.IsActive;
            leaveType.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave Type '{Name}' status changed to {Status}.", leaveType.LeaveTypeName, leaveType.IsActive ? "Active" : "Inactive");
            return (true, null);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            var query = _context.LeaveTypes.Where(lt => lt.LeaveTypeName.ToLower() == normalized);
            if (excludeId.HasValue)
            {
                query = query.Where(lt => lt.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }
    }
}

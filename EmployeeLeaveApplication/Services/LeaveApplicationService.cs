using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class LeaveApplicationService : ILeaveApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveApplicationService> _logger;

        public LeaveApplicationService(ApplicationDbContext context, ILogger<LeaveApplicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public decimal CalculateDays(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                return 0;
            }
            return (decimal)((toDate.Date - fromDate.Date).TotalDays + 1);
        }

        public async Task<LeaveBalanceSummaryDto> GetEmployeeLeaveBalanceAsync(int employeeId, int leaveTypeId, int year)
        {
            var leaveType = await _context.LeaveTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(lt => lt.Id == leaveTypeId);

            if (leaveType == null)
            {
                return new LeaveBalanceSummaryDto();
            }

            var usedDays = await _context.LeaveApplications
                .AsNoTracking()
                .Where(la => la.EmployeeId == employeeId &&
                             la.LeaveTypeId == leaveTypeId &&
                             la.Status == "Approved" &&
                             la.FromDate.Year == year)
                .SumAsync(la => (decimal?)la.NumberOfDays) ?? 0m;

            var allocated = leaveType.AnnualAllocation;
            var available = Math.Max(0m, allocated - usedDays);

            return new LeaveBalanceSummaryDto
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                Allocated = allocated,
                Used = usedDays,
                Available = available
            };
        }

        public async Task<List<LeaveBalanceSummaryDto>> GetAllBalancesForEmployeeAsync(int employeeId, int year)
        {
            var activeLeaveTypes = await _context.LeaveTypes
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .ToListAsync();

            var approvedLeaves = await _context.LeaveApplications
                .AsNoTracking()
                .Where(la => la.EmployeeId == employeeId &&
                             la.Status == "Approved" &&
                             la.FromDate.Year == year)
                .GroupBy(la => la.LeaveTypeId)
                .Select(g => new { LeaveTypeId = g.Key, Used = g.Sum(x => x.NumberOfDays) })
                .ToDictionaryAsync(x => x.LeaveTypeId, x => x.Used);

            return activeLeaveTypes.Select(lt =>
            {
                var used = approvedLeaves.TryGetValue(lt.Id, out var u) ? u : 0m;
                return new LeaveBalanceSummaryDto
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = lt.Id,
                    LeaveTypeName = lt.LeaveTypeName,
                    Allocated = lt.AnnualAllocation,
                    Used = used,
                    Available = Math.Max(0m, lt.AnnualAllocation - used)
                };
            }).ToList();
        }

        public async Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime fromDate, DateTime toDate, int? excludeApplicationId = null)
        {
            var query = _context.LeaveApplications
                .AsNoTracking()
                .Where(la => la.EmployeeId == employeeId &&
                             la.Status != "Rejected" &&
                             la.FromDate.Date <= toDate.Date &&
                             la.ToDate.Date >= fromDate.Date);

            if (excludeApplicationId.HasValue)
            {
                query = query.Where(la => la.Id != excludeApplicationId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> ApplyAsync(LeaveApplicationCreateViewModel model)
        {
            // 1. Validate FromDate cannot be greater than ToDate
            if (model.FromDate.Date > model.ToDate.Date)
            {
                return (false, "From Date cannot be greater than To Date.");
            }

            // 2. Validate FromDate cannot be earlier than current date
            if (model.FromDate.Date < DateTime.Today)
            {
                return (false, "From Date cannot be earlier than the current date.");
            }

            // 3. Reason is mandatory
            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return (false, "Reason for leave is mandatory.");
            }

            // 4. Inactive employees cannot apply
            var employee = await _context.Employees.FindAsync(model.EmployeeId);
            if (employee == null || !employee.IsActive)
            {
                return (false, "Inactive or non-existent employees cannot apply for leave.");
            }

            // 5. Inactive leave types cannot be selected
            var leaveType = await _context.LeaveTypes.FindAsync(model.LeaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
            {
                return (false, "The selected leave type is inactive or invalid.");
            }

            // 6. Recalculate NumberOfDays on server (never trust client)
            var calculatedDays = CalculateDays(model.FromDate, model.ToDate);
            if (calculatedDays <= 0)
            {
                return (false, "Leave duration must be at least 1 day.");
            }

            // 7. Prevent overlapping leave dates (checking Pending and Approved)
            if (await HasOverlappingLeaveAsync(model.EmployeeId, model.FromDate, model.ToDate))
            {
                return (false, "Overlapping leave dates detected. A pending or approved leave already covers the selected dates.");
            }

            // 8. Balance check: Requested days cannot exceed available leave balance
            var balance = await GetEmployeeLeaveBalanceAsync(model.EmployeeId, model.LeaveTypeId, model.FromDate.Year);
            if (calculatedDays > balance.Available)
            {
                return (false, $"Requested leave duration ({calculatedDays} days) exceeds available balance ({balance.Available} days).");
            }

            // 9. Initial status must always be Pending
            var leaveApplication = new LeaveApplication
            {
                EmployeeId = model.EmployeeId,
                LeaveTypeId = model.LeaveTypeId,
                FromDate = model.FromDate.Date,
                ToDate = model.ToDate.Date,
                NumberOfDays = calculatedDays,
                Reason = model.Reason.Trim(),
                Status = "Pending",
                AppliedDate = DateTime.Now,
                ApprovedDate = null,
                ApprovedBy = null,
                RejectionRemarks = null
            };

            _context.LeaveApplications.Add(leaveApplication);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave application created: EmployeeId={EmpId}, TypeId={TypeId}, Days={Days}",
                leaveApplication.EmployeeId, leaveApplication.LeaveTypeId, calculatedDays);

            return (true, null);
        }

        public async Task<LeaveApplicationListViewModel> GetApplicationsAsync(
            int? employeeId,
            int? leaveTypeId,
            string? status,
            int? year)
        {
            var query = _context.LeaveApplications
                .AsNoTracking()
                .Include(la => la.Employee)
                .Include(la => la.LeaveType)
                .Include(la => la.Approver)
                .AsQueryable();

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(la => la.EmployeeId == employeeId.Value);
            }

            if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
            {
                query = query.Where(la => la.LeaveTypeId == leaveTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(la => la.Status == status);
            }

            if (year.HasValue && year.Value > 0)
            {
                query = query.Where(la => la.FromDate.Year == year.Value);
            }

            var items = await query
                .OrderByDescending(la => la.AppliedDate)
                .Select(la => new LeaveApplicationItemViewModel
                {
                    Id = la.Id,
                    EmployeeId = la.EmployeeId,
                    EmployeeName = la.Employee != null ? la.Employee.EmployeeName : string.Empty,
                    EmployeeCode = la.Employee != null ? la.Employee.EmployeeCode : string.Empty,
                    LeaveTypeName = la.LeaveType != null ? la.LeaveType.LeaveTypeName : string.Empty,
                    FromDate = la.FromDate,
                    ToDate = la.ToDate,
                    NumberOfDays = la.NumberOfDays,
                    Reason = la.Reason,
                    Status = la.Status,
                    RejectionRemarks = la.RejectionRemarks,
                    AppliedDate = la.AppliedDate,
                    ApprovedDate = la.ApprovedDate,
                    ApproverName = la.Approver != null ? la.Approver.Username : null
                })
                .ToListAsync();

            return new LeaveApplicationListViewModel
            {
                Applications = items,
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                StatusFilter = status,
                Year = year,
                EmployeeList = await GetActiveEmployeesSelectListAsync(),
                LeaveTypeList = await GetActiveLeaveTypesSelectListAsync()
            };
        }

        public async Task<List<SelectListItem>> GetActiveEmployeesSelectListAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.EmployeeName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.EmployeeName} ({e.EmployeeCode})"
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetActiveLeaveTypesSelectListAsync()
        {
            return await _context.LeaveTypes
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .OrderBy(lt => lt.LeaveTypeName)
                .Select(lt => new SelectListItem
                {
                    Value = lt.Id.ToString(),
                    Text = $"{lt.LeaveTypeName} (Allocation: {lt.AnnualAllocation:0.##} days)"
                })
                .ToListAsync();
        }
    }
}

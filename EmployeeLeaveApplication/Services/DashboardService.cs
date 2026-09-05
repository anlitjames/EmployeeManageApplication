using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardMetricsAsync()
        {
            var today = DateTime.Today;

            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);

            var pendingApplications = await _context.LeaveApplications.CountAsync(la => la.Status == "Pending");
            var approvedApplications = await _context.LeaveApplications.CountAsync(la => la.Status == "Approved");
            var rejectedApplications = await _context.LeaveApplications.CountAsync(la => la.Status == "Rejected");

            var onLeaveToday = await _context.LeaveApplications
                .AsNoTracking()
                .Include(la => la.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(la => la.LeaveType)
                .Where(la => la.Status == "Approved" && la.FromDate <= today && la.ToDate >= today)
                .Select(la => new EmployeeOnLeaveItemViewModel
                {
                    EmployeeId = la.EmployeeId,
                    EmployeeName = la.Employee != null ? la.Employee.EmployeeName : string.Empty,
                    EmployeeCode = la.Employee != null ? la.Employee.EmployeeCode : string.Empty,
                    DepartmentName = la.Employee != null && la.Employee.Department != null ? la.Employee.Department.DepartmentName : "—",
                    LeaveTypeName = la.LeaveType != null ? la.LeaveType.LeaveTypeName : "—",
                    FromDate = la.FromDate,
                    ToDate = la.ToDate,
                    NumberOfDays = la.NumberOfDays
                })
                .ToListAsync();

            var recent = await _context.LeaveApplications
                .AsNoTracking()
                .Include(la => la.Employee)
                .Include(la => la.LeaveType)
                .OrderByDescending(la => la.AppliedDate)
                .Take(5)
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
                    AppliedDate = la.AppliedDate
                })
                .ToListAsync();

            return new DashboardViewModel
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                PendingApplications = pendingApplications,
                ApprovedApplications = approvedApplications,
                RejectedApplications = rejectedApplications,
                EmployeesCurrentlyOnLeave = onLeaveToday,
                RecentApplications = recent
            };
        }
    }
}

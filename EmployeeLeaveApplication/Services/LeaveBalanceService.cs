using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ApplicationDbContext _context;

        public LeaveBalanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveBalanceListViewModel> GetLeaveBalancesAsync(int? employeeId, int? leaveTypeId, int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;

            var employeesQuery = _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                employeesQuery = employeesQuery.Where(e => e.Id == employeeId.Value);
            }

            var employees = await employeesQuery.OrderBy(e => e.EmployeeCode).ToListAsync();

            var leaveTypesQuery = _context.LeaveTypes
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .AsQueryable();

            if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
            {
                leaveTypesQuery = leaveTypesQuery.Where(lt => lt.Id == leaveTypeId.Value);
            }

            var leaveTypes = await leaveTypesQuery.OrderBy(lt => lt.LeaveTypeName).ToListAsync();

            // Aggregate only APPROVED leaves for the target year
            var approvedLeaves = await _context.LeaveApplications
                .AsNoTracking()
                .Where(la => la.Status == "Approved" && la.FromDate.Year == targetYear)
                .GroupBy(la => new { la.EmployeeId, la.LeaveTypeId })
                .Select(g => new
                {
                    g.Key.EmployeeId,
                    g.Key.LeaveTypeId,
                    UsedDays = g.Sum(x => x.NumberOfDays)
                })
                .ToDictionaryAsync(x => (x.EmployeeId, x.LeaveTypeId), x => x.UsedDays);

            var items = new List<LeaveBalanceItemViewModel>();

            foreach (var emp in employees)
            {
                foreach (var lt in leaveTypes)
                {
                    var used = approvedLeaves.TryGetValue((emp.Id, lt.Id), out var u) ? u : 0m;
                    var available = Math.Max(0m, lt.AnnualAllocation - used);

                    items.Add(new LeaveBalanceItemViewModel
                    {
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.EmployeeName,
                        DepartmentName = emp.Department?.DepartmentName ?? "—",
                        LeaveTypeId = lt.Id,
                        LeaveTypeName = lt.LeaveTypeName,
                        Allocated = lt.AnnualAllocation,
                        Used = used,
                        Available = available
                    });
                }
            }

            var allEmployees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.EmployeeName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.EmployeeName} ({e.EmployeeCode})"
                })
                .ToListAsync();

            var allLeaveTypes = await _context.LeaveTypes
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .OrderBy(lt => lt.LeaveTypeName)
                .Select(lt => new SelectListItem
                {
                    Value = lt.Id.ToString(),
                    Text = lt.LeaveTypeName
                })
                .ToListAsync();

            var currentYear = DateTime.Today.Year;
            var yearList = Enumerable.Range(currentYear - 2, 5)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == targetYear
                })
                .ToList();

            return new LeaveBalanceListViewModel
            {
                Balances = items,
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                Year = targetYear,
                EmployeeList = allEmployees,
                LeaveTypeList = allLeaveTypes,
                YearList = yearList
            };
        }
    }
}

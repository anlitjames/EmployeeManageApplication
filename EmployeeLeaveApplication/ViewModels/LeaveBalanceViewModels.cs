using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveApplication.ViewModels
{
    public class LeaveBalanceItemViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Used { get; set; }
        public decimal Available { get; set; }
    }

    public class LeaveBalanceListViewModel
    {
        public List<LeaveBalanceItemViewModel> Balances { get; set; } = new();
        public int? EmployeeId { get; set; }
        public int? LeaveTypeId { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> EmployeeList { get; set; } = new();
        public List<SelectListItem> LeaveTypeList { get; set; } = new();
        public List<SelectListItem> YearList { get; set; } = new();
    }
}

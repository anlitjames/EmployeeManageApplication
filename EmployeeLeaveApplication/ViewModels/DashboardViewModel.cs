namespace EmployeeLeaveApplication.ViewModels
{
    public class EmployeeOnLeaveItemViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NumberOfDays { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int EmployeesOnLeaveTodayCount => EmployeesCurrentlyOnLeave.Count;
        public List<EmployeeOnLeaveItemViewModel> EmployeesCurrentlyOnLeave { get; set; } = new();
        public List<LeaveApplicationItemViewModel> RecentApplications { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.ViewModels
{
    public class PendingLeaveItemViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    public class LeaveApprovalListViewModel
    {
        public List<PendingLeaveItemViewModel> PendingApplications { get; set; } = new();
        public string UserRole { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
    }

    public class RejectLeaveViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Rejection remarks are mandatory.")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        [Display(Name = "Rejection Remarks")]
        public string RejectionRemarks { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveApplication.ViewModels
{
    public class LeaveBalanceSummaryDto
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Used { get; set; }
        public decimal Available { get; set; }
    }

    public class LeaveApplicationCreateViewModel
    {
        [Required(ErrorMessage = "Please select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please select a leave type.")]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "From Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "To Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Display(Name = "Number of Days")]
        public decimal NumberOfDays { get; set; } = 1;

        [Required(ErrorMessage = "Reason is mandatory.")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        [Display(Name = "Reason for Leave")]
        public string Reason { get; set; } = string.Empty;

        public decimal AvailableBalance { get; set; }

        public List<SelectListItem> EmployeeList { get; set; } = new();
        public List<SelectListItem> LeaveTypeList { get; set; } = new();
    }

    public class LeaveApplicationItemViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? RejectionRemarks { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApproverName { get; set; }
    }

    public class LeaveApplicationListViewModel
    {
        public List<LeaveApplicationItemViewModel> Applications { get; set; } = new();
        public int? EmployeeId { get; set; }
        public int? LeaveTypeId { get; set; }
        public string? StatusFilter { get; set; }
        public int? Year { get; set; }
        public List<SelectListItem> EmployeeList { get; set; } = new();
        public List<SelectListItem> LeaveTypeList { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.Models
{
    public class LeaveApplication
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public decimal NumberOfDays { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(1000)]
        public string? RejectionRemarks { get; set; }

        public DateTime AppliedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public int? ApprovedBy { get; set; }

        // Navigation properties

        public Employee? Employee { get; set; }

        public LeaveType? LeaveType { get; set; }

        public User? Approver { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.Models
{
    public class LeaveType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 999)]
        public decimal AnnualAllocation { get; set; }

        public bool CarryForwardAllowed { get; set; }

        [Range(0, 999)]
        public decimal MaxCarryForwardDays { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public ICollection<LeaveApplication> LeaveApplications { get; set; }
            = new List<LeaveApplication>();
    }
}

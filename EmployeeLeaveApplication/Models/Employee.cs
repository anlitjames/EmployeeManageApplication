using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public DateTime DateOfJoining { get; set; }

        public int? ReportingManagerId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation properties

        public Department? Department { get; set; }

        public Employee? ReportingManager { get; set; }

        public ICollection<Employee> Subordinates { get; set; }
            = new List<Employee>();

        public ICollection<LeaveApplication> LeaveApplications { get; set; }
            = new List<LeaveApplication>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;

        public int? EmployeeId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public Employee? Employee { get; set; }

        public ICollection<LeaveApplication> ApprovedApplications { get; set; }
            = new List<LeaveApplication>();
    }
}

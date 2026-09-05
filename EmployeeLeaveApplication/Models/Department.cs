using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}

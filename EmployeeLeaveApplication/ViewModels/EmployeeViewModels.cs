using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeLeaveApplication.ViewModels
{
    public class EmployeeItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public string? ReportingManagerName { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeListViewModel
    {
        public List<EmployeeItemViewModel> Employees { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public string? StatusFilter { get; set; } = "all";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }

    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(20, ErrorMessage = "Employee Code cannot exceed 20 characters.")]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required.")]
        [StringLength(150, ErrorMessage = "Employee Name cannot exceed 150 characters.")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Date of Joining is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; } = DateTime.Today;

        [Display(Name = "Reporting Manager")]
        public int? ReportingManagerId { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> DepartmentList { get; set; } = new();
        public List<SelectListItem> ManagerList { get; set; } = new();
    }

    public class EmployeeEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(20)]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required.")]
        [StringLength(150)]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Date of Joining is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; }

        [Display(Name = "Reporting Manager")]
        public int? ReportingManagerId { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; }

        public List<SelectListItem> DepartmentList { get; set; } = new();
        public List<SelectListItem> ManagerList { get; set; } = new();
    }

    public class EmployeeDetailsViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public string? ReportingManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<string> Subordinates { get; set; } = new();
    }
}

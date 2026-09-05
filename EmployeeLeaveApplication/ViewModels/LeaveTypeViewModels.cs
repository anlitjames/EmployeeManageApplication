using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.ViewModels
{
    public class LeaveTypeItemViewModel
    {
        public int Id { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal AnnualAllocation { get; set; }
        public bool CarryForwardAllowed { get; set; }
        public decimal MaxCarryForwardDays { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationsCount { get; set; }
    }

    public class LeaveTypeCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Leave Type Name is required.")]
        [StringLength(100, ErrorMessage = "Leave Type Name cannot exceed 100 characters.")]
        [Display(Name = "Leave Type Name")]
        public string LeaveTypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Annual Allocation is required.")]
        [Range(0, 999, ErrorMessage = "Annual Allocation must be between 0 and 999.")]
        [Display(Name = "Annual Allocation (Days)")]
        public decimal AnnualAllocation { get; set; }

        [Display(Name = "Allow Carry Forward")]
        public bool CarryForwardAllowed { get; set; }

        [Range(0, 999, ErrorMessage = "Max Carry Forward Days must be between 0 and 999.")]
        [Display(Name = "Maximum Carry Forward Days")]
        public decimal MaxCarryForwardDays { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!CarryForwardAllowed && MaxCarryForwardDays != 0)
            {
                yield return new ValidationResult(
                    "When Carry Forward is disabled, Maximum Carry Forward Days must be 0.",
                    new[] { nameof(MaxCarryForwardDays) });
            }

            if (CarryForwardAllowed && MaxCarryForwardDays > AnnualAllocation)
            {
                yield return new ValidationResult(
                    "Maximum Carry Forward Days cannot exceed Annual Allocation.",
                    new[] { nameof(MaxCarryForwardDays) });
            }
        }
    }

    public class LeaveTypeEditViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Leave Type Name is required.")]
        [StringLength(100, ErrorMessage = "Leave Type Name cannot exceed 100 characters.")]
        [Display(Name = "Leave Type Name")]
        public string LeaveTypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Annual Allocation is required.")]
        [Range(0, 999, ErrorMessage = "Annual Allocation must be between 0 and 999.")]
        [Display(Name = "Annual Allocation (Days)")]
        public decimal AnnualAllocation { get; set; }

        [Display(Name = "Allow Carry Forward")]
        public bool CarryForwardAllowed { get; set; }

        [Range(0, 999, ErrorMessage = "Max Carry Forward Days must be between 0 and 999.")]
        [Display(Name = "Maximum Carry Forward Days")]
        public decimal MaxCarryForwardDays { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!CarryForwardAllowed && MaxCarryForwardDays != 0)
            {
                yield return new ValidationResult(
                    "When Carry Forward is disabled, Maximum Carry Forward Days must be 0.",
                    new[] { nameof(MaxCarryForwardDays) });
            }

            if (CarryForwardAllowed && MaxCarryForwardDays > AnnualAllocation)
            {
                yield return new ValidationResult(
                    "Maximum Carry Forward Days cannot exceed Annual Allocation.",
                    new[] { nameof(MaxCarryForwardDays) });
            }
        }
    }
}

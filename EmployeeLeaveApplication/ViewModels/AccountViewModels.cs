using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveApplication.ViewModels
{
    public class TestUserCredentialDto
    {
        public string Role { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string TestPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        public List<TestUserCredentialDto> TestCredentials { get; set; } = new();
    }
}

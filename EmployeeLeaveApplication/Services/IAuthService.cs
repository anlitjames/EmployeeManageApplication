using EmployeeLeaveApplication.Models;

namespace EmployeeLeaveApplication.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        string HashPassword(User user, string password);
        bool VerifyPassword(User user, string password, string passwordHash);
        Task SeedDefaultUsersAsync();
    }
}

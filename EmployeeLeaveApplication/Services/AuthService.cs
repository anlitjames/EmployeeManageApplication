using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.Models;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
        }

        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower() && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User '{Username}' not found or inactive.", username);
                return null;
            }

            if (!VerifyPassword(user, password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed: Invalid credentials for user '{Username}'.", username);
                return null;
            }

            return user;
        }

        public async Task SeedDefaultUsersAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                return;
            }

            _logger.LogInformation("No users found. Seeding default Admin and Manager accounts...");

            var empAdmin = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP003") 
                           ?? await _context.Employees.FirstOrDefaultAsync();

            var empMgr1 = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP002");
            var empMgr2 = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP004");

            var adminUser = new User
            {
                Username = "admin",
                Role = "Admin",
                EmployeeId = empAdmin?.Id,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            adminUser.PasswordHash = HashPassword(adminUser, "Admin@123");

            var mgrUser1 = new User
            {
                Username = "manager.bose",
                Role = "Manager",
                EmployeeId = empMgr1?.Id,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            mgrUser1.PasswordHash = HashPassword(mgrUser1, "Manager@123");

            var mgrUser2 = new User
            {
                Username = "Rahul",
                Role = "Manager",
                EmployeeId = empMgr2?.Id,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            mgrUser2.PasswordHash = HashPassword(mgrUser2, "Manager@123");

            _context.Users.AddRange(adminUser, mgrUser1, mgrUser2);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded default Admin and Manager users.");
        }

        public async Task<List<TestUserCredentialDto>> GetTestUserCredentialsAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "Manager"))
                .OrderBy(u => u.Role == "Admin" ? 0 : 1)
                .ThenBy(u => u.Id)
                .Select(u => new { u.Role, u.Username })
                .ToListAsync();

            return users.Select(u => new TestUserCredentialDto
            {
                Role = u.Role,
                Username = u.Username,
                TestPassword = u.Role == "Admin" ? "Admin@123" : "Manager@123"
            }).ToList();
        }
    }
}

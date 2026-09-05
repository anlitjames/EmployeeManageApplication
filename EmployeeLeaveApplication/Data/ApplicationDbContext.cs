using EmployeeLeaveApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
           DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<LeaveType> LeaveTypes { get; set; }

        public DbSet<LeaveApplication> LeaveApplications { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee Code unique
            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.EmployeeCode)
                .IsUnique();

            // Email unique
            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // Department name unique
            modelBuilder.Entity<Department>()
                .HasIndex(x => x.DepartmentName)
                .IsUnique();

            // Leave type name unique
            modelBuilder.Entity<LeaveType>()
                .HasIndex(x => x.LeaveTypeName)
                .IsUnique();

            // Username unique
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            // Employee -> Department
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee -> Reporting Manager
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ReportingManager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ReportingManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LeaveApplication -> Employee
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(x => x.Employee)
                .WithMany(x => x.LeaveApplications)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // LeaveApplication -> LeaveType
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(x => x.LeaveType)
                .WithMany(x => x.LeaveApplications)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // LeaveApplication -> User (ApprovedBy)
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(x => x.Approver)
                .WithMany(x => x.ApprovedApplications)
                .HasForeignKey(x => x.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Employee
            modelBuilder.Entity<User>()
                .HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

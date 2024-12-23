using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;

namespace ClassLibrary.Models
{
    public class ClassDBContext : DbContext
    {
        public ClassDBContext(DbContextOptions<ClassDBContext> options) : base(options) { }
        public ClassDBContext() { }

        public DbSet<Screen> Screens { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Location relationships
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Admin)
                .WithMany()
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Agency relationships
            modelBuilder.Entity<Agency>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agency>()
                .HasOne(a => a.Admin)
                .WithMany()
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department relationships
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Location)
                .WithMany()
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Agency)
                .WithMany()
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Screen relationships
            modelBuilder.Entity<Screen>()
                .HasOne(s => s.Location)
                .WithMany()
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Screen>()
                .HasOne(s => s.Agency)
                .WithMany()
                .HasForeignKey(s => s.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Screen>()
                .HasOne(s => s.Department)
                .WithMany()
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Screen>()
                .HasOne(s => s.Admin)
                .WithMany()
                .HasForeignKey(s => s.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // NewsItem basic configuration
            modelBuilder.Entity<NewsItem>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<NewsItem>()
                .HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuItem basic configuration
            modelBuilder.Entity<MenuItem>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<MenuItem>()
                .HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial admin
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "admin@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpassword123"),
                    Role = Role.SuperAdmin,
                    DateCreated = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                }
            );
        }
    }
}

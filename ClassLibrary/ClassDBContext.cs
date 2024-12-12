using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClassLibrary.Models.NewsItem;

namespace ClassLibrary
{
    public class ClassDBContext : DbContext
    {
        public ClassDBContext(DbContextOptions<ClassDBContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<MenuItems> MenuItems { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<NewsItemAgency> NewsItemAgencies { get; set; }
        public DbSet<NewsItemScreen> NewsItemScreens { get; set; }
        public DbSet<NewsItemDepartment> NewsItemDepartments { get; set; }
        public DbSet<NewsItemLocation> NewsItemLocations { get; set; }
        public DbSet<AllowedIpAddress> AllowedIpAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Agency-Location relationship
            modelBuilder.Entity<Agency>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // AdminDepartmentLocation configuration
            modelBuilder.Entity<AdminDepartmentLocation>()
                .HasOne(adl => adl.Admin)
                .WithMany(a => a.AdminDepartmentLocations)
                .HasForeignKey(adl => adl.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminDepartmentLocation>()
                .HasOne(adl => adl.Department)
                .WithMany(d => d.AdminDepartmentLocations)
                .HasForeignKey(adl => adl.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminDepartmentLocation>()
                .HasOne(adl => adl.Location)
                .WithMany()
                .HasForeignKey(adl => adl.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department configuration
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Agency)
                .WithMany()
                .HasForeignKey(d => d.AgencyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Location)
                .WithMany(l => l.Departments)
                .HasForeignKey(d => d.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Employee-Department relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed SuperAdmin
            SuperAdminSeeder.SeedSuperAdmin(modelBuilder);
        }
    }

    public static class SuperAdminSeeder
    {
        public static void SeedSuperAdmin(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@system.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
                    Role = Role.SuperAdmin,
                    DateCreated = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    AgencyId = null
                }
            );
        }
    }
}

using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static SystemModels.Models.Screen;

namespace SystemModels.Models
{
    public class InfoDbContext : DbContext
    {
        public InfoDbContext(DbContextOptions<InfoDbContext> options)
            : base(options)
        {
        }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<MenuItems> MenuItems { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<NewsItemAgency> NewsItemAgencies { get; set; }
        public DbSet<NewsItemScreen> NewsItemScreens { get; set; }
        public DbSet<NewsItemDepartment> NewsItemDepartments { get; set; }
        public DbSet<NewsItemLocation> NewsItemLocations { get; set; }
        public DbSet<AllowedIpAddress> AllowedIpAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the one-to-one relationship between Admin and Location
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Location) // Admin has one Location
                .WithOne(l => l.Admin)    // Location has one Admin
                .HasForeignKey<Admin>(a => a.LocationId);
            // Configure the one-to-one relationship between Admin and Screen
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Screen) // Admin has one Screen
                .WithOne(s => s.Admin)  // Screen has one Admin
                .HasForeignKey<Screen>(s => s.AdminId);

            modelBuilder.Entity<Admin>()
                  .HasOne(a => a.Agency)
                      .WithOne(a => a.Admin)
                      .HasForeignKey<Admin>(a => a.AgencyId);
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
            // Configure Department ↔ Agency relationship
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Agency)
                .WithMany(a => a.Departments)
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Location)
                .WithMany(l => l.Departments)
                .HasForeignKey(d => d.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Employee-Department relationship
            modelBuilder.Entity<Employee>()
                 .HasOne(e => e.Departments)
                 .WithMany(d => d.Employees)
                 .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agency>()
                  .HasOne(a => a.Admin)
                  .WithOne(ad => ad.Agency)
                    .HasForeignKey<Agency>(a => a.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

            // Configure MenuItems relationships
            // MenuItems ↔ Agency
            modelBuilder.Entity<MenuItems>()
                .HasOne(m => m.Agency)
                .WithMany(a => a.MenuItems)
                .HasForeignKey(m => m.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuItems ↔ Admin
            modelBuilder.Entity<MenuItems>()
                .HasOne(m => m.Admin)
                .WithMany(a => a.MenuItems)
                .HasForeignKey(m => m.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the NewsItem-Admin relationship
            modelBuilder.Entity<NewsItem>()
                .HasOne(n => n.Admin)
                .WithMany(a => a.NewsItems)
                .HasForeignKey(n => n.AdminId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading delete

            // Configure relationships between NewsItems and related entities
            modelBuilder.Entity<NewsItem>()
                .HasMany(n => n.NewsItemAgencies)
                .WithOne(na => na.NewsItem)
                .HasForeignKey(na => na.NewsItemId);

            modelBuilder.Entity<NewsItem>()
                .HasMany(n => n.NewsItemScreens)
                .WithOne(ns => ns.NewsItem)
                .HasForeignKey(ns => ns.NewsItemId);

            modelBuilder.Entity<NewsItem>()
                .HasMany(n => n.NewsItemDepartments)
                .WithOne(nd => nd.NewsItem)
                .HasForeignKey(nd => nd.NewsItemId);

            modelBuilder.Entity<NewsItem>()
                .HasMany(n => n.NewsItemLocations)
                .WithOne(nl => nl.NewsItem)
                .HasForeignKey(nl => nl.NewsItemId);

            // Configure allowed IP addresses
            modelBuilder.Entity<AllowedIpAddress>()
                .HasKey(aip => aip.IpAddress);

            // Hardcode the single Admin into the database
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpassword123"),  // Store the password hash, not plaintext
                    Email = "admin@company.com",
                    DateCreated = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    Role = Role.Admin,  // Assuming 'Role.Admin' is an enum for admin role
                }
            );
        }
    }
}

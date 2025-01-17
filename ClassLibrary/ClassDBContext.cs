using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;

namespace ClassLibrary.Models
{
    public class ClassDBContext : DbContext
    {
        public ClassDBContext(DbContextOptions<ClassDBContext> options) : base(options) { }
        public ClassDBContext() { }

        public DbSet<Screen> Screens { get; set; }
        public DbSet<ScreenAccess> ScreenAccesses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<DisplayTracker> DisplayTrackers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Location relationships
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Admin)
                .WithMany()
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Locations_Admins_AdminId");

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
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Agencies_Admins_AdminId");

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
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.DateCreated)
                    .IsRequired();
                    
                entity.Property(e => e.LastUpdated)
                    .IsRequired();
                    
                entity.Property(e => e.IsActive)
                    .IsRequired();

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                    .WithMany()
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Employees_Admins_AdminId");
            });

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
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Screens_Admins_AdminId");

            // NewsItem basic configuration
            modelBuilder.Entity<NewsItem>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<NewsItem>()
                .HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.AdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_NewsItems_Admins_AdminId");

            // MenuItem basic configuration
            modelBuilder.Entity<MenuItem>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<MenuItem>()
                .HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.AdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MenuItems_Admins_AdminId");

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

            modelBuilder.Entity<DisplayTracker>()
                .HasOne(dt => dt.Screen)
                .WithMany()
                .HasForeignKey(dt => dt.ScreenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

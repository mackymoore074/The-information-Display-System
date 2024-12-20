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
        public DbSet<MenuItems> MenuItems { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<NewsItemAgency> NewsItemAgencies { get; set; }
        public DbSet<NewsItemScreen> NewsItemScreens { get; set; }
        public DbSet<NewsItemDepartment> NewsItemDepartments { get; set; }
        public DbSet<NewsItemLocation> NewsItemLocations { get; set; }
        public DbSet<AllowedIpAddress> AllowedIpAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Agency relationships - combined into one section
            modelBuilder.Entity<Agency>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department relationships
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Agency)
                .WithMany()
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Location)
                .WithMany(l => l.Departments)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Screen relationships
            modelBuilder.Entity<Screen>()
                .HasOne(s => s.Location)
                .WithMany(l => l.Screens)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // NewsItem relationships
            modelBuilder.Entity<NewsItem>()
                .HasOne(n => n.Admin)
                .WithMany(a => a.NewsItems)
                .HasForeignKey(n => n.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // NewsItem junction tables
            modelBuilder.Entity<NewsItemAgency>()
                .HasKey(na => new { na.NewsItemId, na.AgencyId });

            modelBuilder.Entity<NewsItemDepartment>()
                .HasKey(nd => new { nd.NewsItemId, nd.DepartmentId });

            modelBuilder.Entity<NewsItemLocation>()
                .HasKey(nl => new { nl.NewsItemId, nl.LocationId });

            modelBuilder.Entity<NewsItemScreen>()
                .HasKey(ns => new { ns.NewsItemId, ns.ScreenId });

            // AllowedIpAddress configuration
            modelBuilder.Entity<AllowedIpAddress>()
                .HasKey(a => a.IpAddress);

            // Add this Admin-Agency relationship configuration
            modelBuilder.Entity<Agency>()
                .HasOne(a => a.Admin)
                .WithMany(admin => admin.Agencies)
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add this Admin-Location relationship configuration
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .IsRequired(false) 
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

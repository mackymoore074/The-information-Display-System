using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public Role Role { get; set; }

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastLogin { get; set; }

        // Navigation properties
        public ICollection<AdminDepartmentLocation> AdminDepartmentLocations { get; set; }
        public ICollection<MenuItems> MenuItems { get; set; }
        public ICollection<NewsItem> NewsItems { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }

    // New junction table for Admin-Department-Location relationship
    public class AdminDepartmentLocation
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public DateTime AssignedDate { get; set; }
    }

    // Enum for Role with explicitly set integer values
    public enum Role
    {
        [Display(Name = "Super Administrator")]
        SuperAdmin = 1, // Super admin user
        [Display(Name = "Administrator")]
        Admin = 2 // Admin user
    }
}

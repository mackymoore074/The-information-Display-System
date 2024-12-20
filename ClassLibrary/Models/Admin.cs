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

        [Required]
        public string PasswordHash { get; set; }

        public Role Role { get; set; }

        // Nullable relationships
        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public int? ScreenId { get; set; }
        public Screen? Screen { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastLogin { get; set; }

        // Collections
        public ICollection<MenuItems> MenuItems { get; set; } = new List<MenuItems>();
        public ICollection<NewsItem> NewsItems { get; set; } = new List<NewsItem>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        // Change this to ICollection to indicate one-to-many relationship
        public ICollection<Agency> Agencies { get; set; } = new List<Agency>();
    }

    public enum Role
    {
        [Display(Name = "Super Administrator")]
        SuperAdmin = 1,
        
        [Display(Name = "Administrator")]
        Admin = 2
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    public class Employee
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
        [StringLength(100)]
        public string Email { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        [Required]
        public int AdminId { get; set; }
        public virtual Admin Admin { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; }

        public Employee()
        {
            DateCreated = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            IsActive = true;
        }
    }
}

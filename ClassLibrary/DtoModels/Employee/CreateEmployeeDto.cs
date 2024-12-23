using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Employee
{
    public class CreateEmployeeDto
    {
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

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public int AdminId { get; set; }

        public bool IsActive { get; set; } = true;
    }
} 
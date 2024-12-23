using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Employee
{
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [StringLength(20)]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public int LocationId { get; set; }
    }
} 
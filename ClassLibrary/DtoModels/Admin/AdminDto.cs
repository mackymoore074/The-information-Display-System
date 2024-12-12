using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Admin
{
    public class AdminDto
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Enum converted to string (ensure your Role is an enum)
        public string Role { get; set; }

        // Optional string properties for related entities
        public string AgencyName { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string ScreenName { get; set; }

        // Including DateCreated and LastLogin timestamps
        public DateTime DateCreated { get; set; }
        public DateTime LastLogin { get; set; }
    }


}

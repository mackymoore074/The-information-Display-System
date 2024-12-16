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
        public int? AgencyId { get; set; }
        public string AgencyName { get; set; } // Agency name for display purposes

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } // Department name for display purposes

        public int? LocationId { get; set; }
        public string LocationName { get; set; } // Location name for display purposes

        public int? ScreenId { get; set; }
        public string ScreenName { get; set; } // Screen name for display purposes

        public DateTime DateCreated { get; set; }
        public DateTime LastLogin { get; set; }

    }


}

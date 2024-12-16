using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Department
{
    public class UpdateDepartmentDto
    {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } // Department name

            public string Description { get; set; } // Description of the department

            [Required]
            public int AgencyId { get; set; } // ID of the associated agency

            [Required]
            public int LocationId { get; set; } // ID of the associated location
    }
}

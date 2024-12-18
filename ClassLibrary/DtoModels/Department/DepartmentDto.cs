using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // Department name
        public string Description { get; set; } // Description of the department
        public DateTime DateCreated { get; set; }
        public int AgencyId { get; set; } // ID of the associated agency
        public string AgencyName { get; set; }
        public int LocationId { get; set; } // ID of the associated location
        public string LocationName { get; set; }
    }
}

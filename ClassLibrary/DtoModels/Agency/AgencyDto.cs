using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Agency
{
    public class AgencyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public int AdminId { get; set; }

        public string AdminName { get; set; }

        public int LocationId { get; set; }

        public string LocationName { get; set; }

        public DateTime DateCreated { get; set; }
    }
}

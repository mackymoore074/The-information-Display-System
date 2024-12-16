using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Agency
{
    public class CreateAgencyDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int AdminId { get; set; }

        public int LocationId { get; set; }
    }
}

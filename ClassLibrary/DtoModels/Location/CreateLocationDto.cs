using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Location
{
    public class CreateLocationDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } // Location name

        [Required]
        [StringLength(500)]
        public string Address { get; set; } // Physical address


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    public class Department
    {
        public Department()
        {
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public int? AgencyId { get; set; }  // Optional
        public Agency Agency { get; set; }
        
        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; }

    }
}

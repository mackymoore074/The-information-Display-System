using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    public class Agency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; }

        [Required]
        public int AdminId { get; set; }
        public Admin Admin { get; set; }

        public DateTime DateCreated { get; set; }
    }
}

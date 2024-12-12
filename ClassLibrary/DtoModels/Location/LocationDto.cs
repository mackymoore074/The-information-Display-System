using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Location
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class CreateLocationDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; }
    }

    public class UpdateLocationDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; }
    }
}

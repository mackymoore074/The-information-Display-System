using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Agency
{
    public class AgencyDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class CreateAgencyDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public int LocationId { get; set; }
    }

    public class UpdateAgencyDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public int LocationId { get; set; }
    }
}

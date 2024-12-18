using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Agency
{
    public class UpdateAgencyDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int LocationId { get; set; }
    }
}

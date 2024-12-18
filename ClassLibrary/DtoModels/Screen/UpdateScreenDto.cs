using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Screen
{
    public class UpdateScreenDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public int DepartmentId { get; set; }
        
        [Required]
        public int AgencyId { get; set; }

        [Required]
        public int LocationId { get; set; }

        public string ScreenType { get; set; }
        public bool IsOnline { get; set; }
        public string StatusMessage { get; set; }
        public string MACAddress { get; set; }
    }
}

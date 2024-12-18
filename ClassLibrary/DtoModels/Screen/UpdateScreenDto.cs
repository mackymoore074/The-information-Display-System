using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Screen
{
    public class UpdateScreenDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; }

        [Required(ErrorMessage = "Agency is required")]
        public string AgencyId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string LocationId { get; set; }

        public int AdminId { get; set; }

        [Required(ErrorMessage = "Screen Type is required")]
        public string ScreenType { get; set; }

        public string MACAddress { get; set; }

        public bool IsOnline { get; set; }
        public string StatusMessage { get; set; }
    }
}

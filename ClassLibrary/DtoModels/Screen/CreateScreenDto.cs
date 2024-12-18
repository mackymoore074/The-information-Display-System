using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Screen
{
    public class CreateScreenDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string LocationId { get; set; } = string.Empty;

        [Required]
        public string AgencyId { get; set; } = string.Empty;

        public string? DepartmentId { get; set; }

        public string AdminId { get; set; } = string.Empty;

        public string ScreenType { get; set; } = string.Empty;

        public bool IsOnline { get; set; }

        [Required]
        public string StatusMessage { get; set; } = string.Empty;

        public string MACAddress { get; set; } = string.Empty;
    }
}

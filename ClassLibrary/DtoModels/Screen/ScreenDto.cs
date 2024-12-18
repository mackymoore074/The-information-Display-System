using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Screen
{
    public class ScreenDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public int AgencyId { get; set; }
        public int? DepartmentId { get; set; }
        public int AdminId { get; set; }
        public string ScreenType { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        [Required]
        public string StatusMessage { get; set; } = string.Empty;
        public string MACAddress { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public DateTime LastCheckedIn { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
    }
}

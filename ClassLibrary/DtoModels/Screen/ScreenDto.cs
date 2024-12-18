using System;

namespace ClassLibrary.DtoModels.Screen
{
    public class ScreenDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public int AgencyId { get; set; }
        public int LocationId { get; set; }
        public int AdminId { get; set; }
        public string ScreenType { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastCheckedIn { get; set; }
        public bool IsOnline { get; set; }
        public string StatusMessage { get; set; }
        public string MACAddress { get; set; }
        public string LocationName { get; set; }
        public string DepartmentName { get; set; }
        public string AgencyName { get; set; }
    }
}

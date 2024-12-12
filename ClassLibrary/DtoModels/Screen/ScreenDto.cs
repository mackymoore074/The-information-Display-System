using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Screen
{
    public class ScreenDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ScreenType { get; set; }
        public bool IsOnline { get; set; }
        public string StatusMessage { get; set; }
        public string MACAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastCheckedIn { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
    }
}

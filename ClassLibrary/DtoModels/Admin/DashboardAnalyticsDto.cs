using ClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Admin
{
    public class DashboardAnalyticsDto
    {
        public int TotalScreens { get; set; }
        public int ActiveScreens { get; set; }
        public int TotalMenuItems { get; set; }
        public int TotalNewsItems { get; set; }
        public List<ItemDisplayStats> TopDisplayedMenuItems { get; set; }
        public List<ItemDisplayStats> TopDisplayedNewsItems { get; set; }
        public List<ScreenActivityStats> ScreenActivities { get; set; }
    }

    public class ItemDisplayStats
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public int DisplayCount { get; set; }
        public DateTime LastDisplayed { get; set; }
    }

    public class ScreenActivityStats
    {
        public int ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string Location { get; set; }
        public int TotalDisplays { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsCurrentlyActive { get; set; }
    } 
}
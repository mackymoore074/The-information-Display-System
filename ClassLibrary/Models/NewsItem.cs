using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    public class NewsItem
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(450)]
        public string NewsItemBody { get; set; } // News item body

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime TimeOutDate { get; set; }

        public ImportanceLevel Importance { get; set; }

        [Url]
        public string MoreInformationUrl { get; set; }
        // Foreign Key to Admin (to track the admin who created the news item)
        public int AdminId { get; set; }
        public Admin Admin { get; set; }  // Navigation property to Admin

        // Navigation Properties for Many-to-Many
        public ICollection<NewsItemAgency> NewsItemAgencies { get; set; }
        public ICollection<NewsItemScreen> NewsItemScreens { get; set; }
        public ICollection<NewsItemDepartment> NewsItemDepartments { get; set; }
        public ICollection<NewsItemLocation> NewsItemLocations { get; set; }

        public NewsItem()
        {
            NewsItemAgencies = new List<NewsItemAgency>();
            NewsItemScreens = new List<NewsItemScreen>();
            NewsItemDepartments = new List<NewsItemDepartment>();
            NewsItemLocations = new List<NewsItemLocation>();
        }

        public enum ImportanceLevel
        {
            VeryImportant = 1,
            SomewhatImportant = 2,
            LowImportance = 3
        }
    }

    // Join Table Models
    public class NewsItemAgency
    {
        public int Id { get; set; }
        public int NewsItemId { get; set; }
        public NewsItem NewsItem { get; set; }

        public int AgencyId { get; set; }
        public Agency Agency { get; set; }
    }

    public class NewsItemScreen
    {
        public int Id { get; set; }
        public int NewsItemId { get; set; }
        public NewsItem NewsItem { get; set; }

        public int ScreenId { get; set; }
        public Screen Screen { get; set; }
    }

    public class NewsItemDepartment
    {
        public int Id { get; set; }
        public int NewsItemId { get; set; }
        public NewsItem NewsItem { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }

    public class NewsItemLocation
    {
        public int Id { get; set; }
        public int NewsItemId { get; set; }
        public NewsItem NewsItem { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}

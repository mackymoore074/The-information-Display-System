using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.NewsItem
{
    // DTO for NewsItem
    public class NewsItemDto
    {
        public int Id { get; set; }
        public int AdminId { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(450)]
        public string NewsItemBody { get; set; } // News item body

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime TimeOutDate { get; set; }

        public ImportanceLevelDto Importance { get; set; }

        [Url]
        public string MoreInformationUrl { get; set; }

        // Related data (IDs of related entities)
        public List<int> AgencyId { get; set; }
        public List<int> DepartmentId { get; set; }
        public List<int> LocationId { get; set; }
        public List<int> ScreenId { get; set; }


        public NewsItemDto()
        {
            AgencyId = new List<int>();
            DepartmentId = new List<int>();
            LocationId = new List<int>();
            ScreenId = new List<int>();
        }

        public enum ImportanceLevelDto
        {
            VeryImportant = 1,
            SomewhatImportant = 2,
            LowImportance = 3
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ClassLibrary.Models.NewsItem;

namespace ClassLibrary.DtoModels.NewsItem
{
    public class CreateNewsItemDto
    {
        public int AdminId { get; set; }
        [StringLength(450)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string NewsItemBody { get; set; }

        public DateTime TimeOutDate { get; set; }

        [Required]
        public ImportanceLevel Importance { get; set; } // Use Enum instead of string

        [Url]
        public string MoreInformationUrl { get; set; }
        // New property to track the Admin that created the news

        // Related entity identifiers
        public List<int> AgencyId { get; set; }
        public List<int> ScreenId { get; set; }
        public List<int> DepartmentId { get; set; }
        public List<int> LocationId { get; set; }

        public CreateNewsItemDto()
        {
            AgencyId = new List<int>();
            ScreenId = new List<int>();
            DepartmentId = new List<int>();
            LocationId = new List<int>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ClassLibrary.Models.NewsItem;

namespace ClassLibrary.DtoModels.NewsItem
{
    public class UpdateNewsItemDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(450)]
        public string NewsItemBody { get; set; } // News item body

        public DateTime TimeOutDate { get; set; }

        [Required]
        public ImportanceLevel Importance { get; set; } // Use Enum instead of int

        [Url]
        public string MoreInformationUrl { get; set; }

        // New property to track the Admin that Updated the news
        public int AdminId { get; set; }

        public List<int> AgencyId { get; set; } = new List<int>();
        public List<int> DepartmentId { get; set; } = new List<int>();
        public List<int> LocationId { get; set; } = new List<int>();
        public List<int> ScreenId { get; set; } = new List<int>();
    }
}

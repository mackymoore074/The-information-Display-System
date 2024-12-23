using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClassLibrary.Models;

namespace ClassLibrary.DtoModels.NewsItem
{
    public class CreateNewsItemDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(450)]
        public string NewsItemBody { get; set; }

        [Required]
        public DateTime TimeOutDate { get; set; }

        public ImportanceLevel Importance { get; set; }

        [Url]
        public string? MoreInformationUrl { get; set; }

        public List<int> Departments { get; set; } = new();
        public List<int> Screens { get; set; } = new();
        public List<int> Locations { get; set; } = new();
    }
}

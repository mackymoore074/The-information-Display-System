using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClassLibrary.Models;

namespace ClassLibrary.DtoModels.MenuItem
{
    public class CreateMenuItemDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(450)]
        public string Description { get; set; }

        [Required]
        public DateTime TimeOutDate { get; set; }

        public MealType Type { get; set; }

        [Range(0, 10000)]
        public decimal? Price { get; set; }

        public List<int> Departments { get; set; } = new();
        public List<int> Screens { get; set; } = new();
        public List<int> Locations { get; set; } = new();
    }
}

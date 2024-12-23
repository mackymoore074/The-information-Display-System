using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClassLibrary.Models;

namespace ClassLibrary.DtoModels.MenuItem
{
    public class CreateMenuItemDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public MealType Type { get; set; }

        [Required]
        public DateTime TimeOutDate { get; set; }

        [Required]
        public int AdminId { get; set; }

        public List<int> Departments { get; set; } = new();
        public List<int> Locations { get; set; } = new();
        public List<int> Screens { get; set; } = new();
    }
}

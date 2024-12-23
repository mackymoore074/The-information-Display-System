using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClassLibrary.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(450)]
        public string Description { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        
        [Required]
        public DateTime TimeOutDate { get; set; }  // When the menu item should stop being displayed

        public MealType Type { get; set; }  // Breakfast, Lunch, Dinner, etc.

        [Range(0, 10000)]
        public decimal? Price { get; set; }

        [Required]
        public int AdminId { get; set; }
        public Admin Admin { get; set; }

        public bool IsActive { get; set; }

        // Store arrays as JSON strings in the database
        public string DepartmentIds { get; set; } = "[]";
        public string ScreenIds { get; set; } = "[]";
        public string LocationIds { get; set; } = "[]";

        // Add [NotMapped] to these properties
        [NotMapped]
        public List<int> Departments 
        {
            get => string.IsNullOrEmpty(DepartmentIds) 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(DepartmentIds);
            set => DepartmentIds = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<int> Screens
        {
            get => string.IsNullOrEmpty(ScreenIds) 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(ScreenIds);
            set => ScreenIds = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<int> Locations
        {
            get => string.IsNullOrEmpty(LocationIds) 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(LocationIds);
            set => LocationIds = JsonSerializer.Serialize(value);
        }

        public MenuItem()
        {
            DateCreated = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            IsActive = true;
            Departments = new List<int>();
            Screens = new List<int>();
            Locations = new List<int>();
        }
    }

    public enum MealType
    {
        Breakfast = 1,
        Lunch = 2,
        Dinner = 3,
        Snack = 4,
        Dessert = 5,
        Beverage = 6
    }
}

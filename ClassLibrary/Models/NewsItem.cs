﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClassLibrary.Models
{
    public class NewsItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(450)]
        public string NewsItemBody { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        
        [Required]
        public DateTime TimeOutDate { get; set; }

        public ImportanceLevel Importance { get; set; }

        [Url]
        public string? MoreInformationUrl { get; set; }

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

        public NewsItem()
        {
            DateCreated = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            IsActive = true;
            Departments = new List<int>();
            Screens = new List<int>();
            Locations = new List<int>();
        }
    }

    public enum ImportanceLevel
    {
        VeryImportant = 1,
        SomewhatImportant = 2,
        LowImportance = 3
    }
}
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class Screen
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [JsonPropertyName("name")]
        public string Name { get; set; } // Unique screen name (e.g., "DM001", "LH003")

        [JsonPropertyName("locationId")]
        public int LocationId { get; set; } // Foreign key to Location
        public Location Location { get; set; } // Navigation property

        // Assuming many-to-many relationship for departments

        public int AgencyId { get; set; } // Foreign key to Agency
        public Agency Agency { get; set; } // Navigation property
        public DateTime DateCreated { get; set; } // Date and time when the screen was created
        public int AdminId { get; set; } // Admin who created the screen
        public Admin Admin { get; set; } // Navigation property for Admin
        public int? DepartmentId { get; set; } // Foreign key to Department
        public Department? Department { get; set; } // Navigation property

        [Required]
        public string ScreenType { get; set; } // Type of screen (e.g., "TV", "LED")
        public DateTime LastUpdated { get; set; } // Last time this screen was updated
        public DateTime LastCheckedIn { get; set; } // Date and time when the screen last checked in
        public bool IsOnline { get; set; } // Whether the screen is currently online or offline
        public string StatusMessage { get; set; } // A message regarding the screen status
        public string MACAddress { get; set; } // MAC address of the screen

    }


}


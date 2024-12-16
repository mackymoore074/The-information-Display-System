﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SystemModels.Models
{
    public class CreateScreenDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } // Unique screen name (e.g., "DM001", "LH003")

        public int LocationId { get; set; } // Foreign key to Location
        public int DepartmentId { get; set; } // Foreign key to Department
        public int AgencyId { get; set; } // Foreign key to Agency
        public int AdminId { get; set; } // Foreign key to Admin

        [Required]
        public string ScreenType { get; set; } // Type of screen (e.g., "TV", "LED")

        public bool IsOnline { get; set; } // Whether the screen is currently online or offline
        public string StatusMessage { get; set; } // A message regarding the screen status
        public string MACAddress { get; set; } // MAC address of the screen
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models

{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } // Location name (e.g., "Foothill (Finance)")

        [Required]
        [StringLength(500)]
        public string Address { get; set; } // Physical address of the location

        public DateTime DateCreated { get; set; }  // Date the location was created
        public ICollection<Admin> Admins { get; set; } // Admins linked to this location
        public List<Screen> Screens { get; set; } = new List<Screen>(); // Screens in this location
        public ICollection<NewsItemLocation> NewsItemLocations { get; set; }// Many-to-many relationship with NewsItems
        public List<Department> Departments { get; set; } = new List<Department>(); // Departments in this location
        public List<AllowedIpAddress> AllowedIpAddresses { get; set; } = new List<AllowedIpAddress>();// Public IPs allowed to retrieve news

        // Constructor
        public Location()
        {
            AllowedIpAddresses = new List<AllowedIpAddress>(); // Initialize the collection
            NewsItemLocations = new List<NewsItemLocation>(); // Initialize the collection

        }
    }
}


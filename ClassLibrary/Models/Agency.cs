using ClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{

    public class Agency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public ICollection<Department> Departments { get; set; }
        public ICollection<MenuItems> MenuItems { get; set; }
        public DateTime DateCreated { get; set; }

        public Agency()
        {
            Departments = new List<Department>();
            MenuItems = new List<MenuItems>();

        }
    }
}

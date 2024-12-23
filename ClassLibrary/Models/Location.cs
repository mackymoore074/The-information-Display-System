using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibrary.Models

{
    public class Location
    {
       public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public DateTime DateCreated { get; set; }

        [Required]
        public int AdminId { get; set; }
        public virtual Admin Admin { get; set; }
    }
}


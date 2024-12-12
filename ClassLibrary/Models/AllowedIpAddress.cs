using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class AllowedIpAddress
    {
        [Key]
        public string IpAddress { get; set; }
        public int locationId { get; set; }
        public Location Location { get; set; }
    }
}

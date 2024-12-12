using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class SignInRequest
    {
        public string MACAddress { get; set; }  // MAC address of the device
        public string Password { get; set; } // Password of the user
    }
}

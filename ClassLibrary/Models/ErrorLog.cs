using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public int ScreenId { get; set; } // Screen ID where the error occurred
        public string ErrorMessage { get; set; } // Details Error message
        public DateTime DateCreated { get; set; } // When the error occurred
    }
}

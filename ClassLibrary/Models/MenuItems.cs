using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemModels.Models;

namespace ClassLibrary.Models
{
    public class MenuItems
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExpired { get; set; }
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

    }
}

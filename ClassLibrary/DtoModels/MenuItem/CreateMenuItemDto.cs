using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.MenuItem
{
    public class CreateMenuItemDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsExpired { get; set; }
        public DateTime EndDate { get; set; }
        public int AgencyId { get; set; }
        public int AdminId { get; set; }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Location
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // Location name
        public string Address { get; set; } // Physical address
        public DateTime DateCreated { get; set; } // Date the location was created


    }
}

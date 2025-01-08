using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClassLibrary.Models {
    public class DisplayTracker
    {
        public int Id { get; set; }
        public int ScreenId { get; set; }
        public string ItemType { get; set; }  // "MenuItem" or "NewsItem"
        public int ItemId { get; set; }
        public DateTime DisplayedAt { get; set; }
        
        [JsonIgnore]
        public virtual Screen? Screen { get; set; }
    } 
}
using System.Text.Json.Serialization;

namespace ClassLibrary.DtoModels.Location
{
    public class LocationDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } // Location name
        [JsonPropertyName("address")]
        public string Address { get; set; } // Physical address
        [JsonPropertyName("datecreated")]
        public DateTime DateCreated { get; set; } // Date the location was created


    }
}

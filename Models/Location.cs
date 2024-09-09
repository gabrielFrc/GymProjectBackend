using System.Text.Json.Serialization;

namespace GymProjectBackend.Models{
    public class Location()
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        [JsonPropertyName("display_name")]
        public required string Display_Name { get; init; } // Usar PascalCase

        [JsonPropertyName("name")]
        public required string Name { get; init; } // Usar PascalCase

        [JsonPropertyName("lat")]
        public required string Lat { get; set; } // Usar PascalCase

        [JsonPropertyName("lon")]
        public required string Lon { get; set; } // Usar PascalCase

        [JsonPropertyName("distance")]
        public double Distance { get; init; } = 0; // Usar PascalCase
    }
}
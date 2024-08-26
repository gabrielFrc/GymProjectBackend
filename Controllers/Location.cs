using System.Text.Json;
using GymProjectBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymProjectBackend.Controllers
{
    [ApiController]
    [Route("locate")]
    public class LocationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LocationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult GetAllLocations()
        {
            List<Location> storeLocation = new List<Location>();

            for (int i = 0; i < Ls.GetLength(0); i++)
            {
                Location myLocal = new Location
                {
                    Display_Name = (string)Ls[i, 3],
                    Name = (string)Ls[i, 2],
                    Lat = Ls[i, 0]?.ToString() ?? "0.00",
                    Lon = Ls[i, 1]?.ToString() ?? "0.00",
                    Distance = 0.00,
                };
                storeLocation.Add(myLocal);
            }
            return Ok(storeLocation.ToArray());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocations(string id)
        {
            var locations = await GetLocationsFromNominatim(id);
            if (locations == null)
            {
                return NotFound();
            }

            return Ok(locations);
        }

        private async Task<Location[]?> GetLocationsFromNominatim(string id)
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={id}&format=json";

            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "YourAppName/1.0 (your.email@example.com)");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonObj = JsonSerializer.Deserialize<Location[]>(content);

                    if (jsonObj == null || jsonObj.Length <= 0)
                    {
                        return null;
                    }

                    jsonObj[0].Lat = jsonObj[0].Lat.Replace('.', ',');
                    jsonObj[0].Lon = jsonObj[0].Lon.Replace('.', ',');

                    List<Location> storeLocation = new List<Location>();

                    for (int i = 0; i < Ls.GetLength(0); i++)
                    {
                        double localDistance = Haversine(double.Parse(jsonObj[0].Lat), double.Parse(jsonObj[0].Lon),
                            (double)Ls[i, 0], (double)Ls[i, 1]);
                        Location myLocal = new Location
                        {
                            Display_Name = (string)Ls[i, 3],
                            Name = (string)Ls[i, 2],
                            Lat = Ls[i, 0]?.ToString() ?? "0.00",
                            Lon = Ls[i, 1]?.ToString() ?? "0.00",
                            Distance = Math.Round(localDistance, 2)
                        };
                        if (i == 0)
                        {
                            storeLocation.Add(myLocal);
                        }
                        else
                        {
                            for (int j = 0; j < storeLocation.Count; j++)
                            {
                                if (localDistance < storeLocation[j].Distance)
                                {
                                    storeLocation.Insert(j, myLocal);
                                    break;
                                }
                                else if (j == storeLocation.Count - 1)
                                {
                                    storeLocation.Add(myLocal);
                                    break;
                                }
                            }
                        }
                    }
                    return storeLocation.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in km
        }

        private double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }

        // Defina sua matriz Ls aqui ou recupere-a de outra forma
        private readonly object[,] Ls = {
            {-22.971974, -43.1842997, "Rio de Janeiro", "Um lugar apaziguado neste..."},
            {-23.5506507, -46.6333824, "São Paulo", "Um lugar apaziguado neste..."},
            {-18.57712805, -45.18445836790818, "Minas Gerais", "Um lugar apaziguado neste..."},
        };
    }
}
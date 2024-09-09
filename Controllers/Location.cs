using System.Text.Json;
using GymProjectBackend.Models;
using GymProjectBackend.Functions;
using Microsoft.AspNetCore.Mvc;
using GymProjectBackend.Records;
using GymProjectBackend.Data;

namespace GymProjectBackend.Controllers
{
    [ApiController]
    [Route("locate")]
    public class LocationController : ControllerBase
    {
         // Defina sua matriz fixedLocations aqui ou recupere-a de outra forma
        // Latitude, Longitude
        private readonly object[,] fixedLocations = {
            {-22.971974, -43.1842997, "Rio de Janeiro", "Um lugar apaziguado neste..."},
            {-23.5506507, -46.6333824, "São Paulo", "Um lugar apaziguado neste..."},
            {-18.57712805, -45.18445836790818, "Minas Gerais", "Um lugar apaziguado neste..."},
            {35.689487, 139.691706, "Japan - Tokyo", "Japanese place.."}
        };
        const int pageLimit = 3;
        private readonly IHttpClientFactory _httpClientFactory;

        public LocationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> PostLocation(AddLocationRequest request, AppDbContext context){
            Location Locations = new Location
                        {
                            Display_Name = request.Display_Name,
                            Name = request.Name,
                            Lat = request.Lat,
                            Lon = request.Lon,
                            Distance = request.Distance
                        };
            // Console.WriteLine(loc.Id);
            await context.Locations.AddAsync(Locations);
            await context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet]
        public IActionResult GetAllLocations(AppDbContext context)
        {
            List<Location> locations = context.Locations.ToList<Location>();
            return Ok(locations);

            // List<Location> storeLocation = new List<Location>();

            // for (int i = 0; i < fixedLocations.GetLength(0); i++)
            // {
            //     if(i >= 5){break;}
            //     Location myLocal = new Location
            //     {
            //         Display_Name = (string)fixedLocations[i, 3],
            //         Name = (string)fixedLocations[i, 2],
            //         Lat = fixedLocations[i, 0]?.ToString() ?? "0.00",
            //         Lon = fixedLocations[i, 1]?.ToString() ?? "0.00",
            //         Distance = 0.00,
            //     };
            //     storeLocation.Add(myLocal);
            // }
            // return Ok(storeLocation.ToArray());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocations(string id, [FromQuery] int page)
        {
            if(page * pageLimit > fixedLocations.GetLength(0)){
                return NotFound( new { Message = "All locations has been showed", AllShowed = true });
            }

            var locations = await GetLocationsFromNominatim(id, page * pageLimit);
            if (locations == null)
            {
                return NotFound( new { Message = "Location Not Found" } );
            }

            return Ok(locations);
        }

        private async Task<Location[]?> GetLocationsFromNominatim(string id, int pageQuantity)
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


                    for (int i = pageQuantity; i < fixedLocations.GetLength(0); i++)
                    {
                        if(i - pageQuantity == 3){break;}
                        double localDistance = Haversine.Formula(double.Parse(jsonObj[0].Lat), double.Parse(jsonObj[0].Lon),
                            (double)fixedLocations[i, 0], (double)fixedLocations[i, 1]);
                        Location myLocal = new Location
                        {
                            Display_Name = (string)fixedLocations[i, 3],
                            Name = (string)fixedLocations[i, 2],
                            Lat = fixedLocations[i, 0]?.ToString() ?? "0.00",
                            Lon = fixedLocations[i, 1]?.ToString() ?? "0.00",
                            Distance = Math.Round(localDistance, 2)
                        };
                        if (i == pageQuantity)
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
    }
}
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
                        };
            await context.Locations.AddAsync(Locations);
            await context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet]
        public IActionResult GetAllLocations(AppDbContext context)
        {
            List<Location> locations = context.Locations
            .Take(6)
            .ToList<Location>();

            return Ok(locations);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocations2(string id, [FromQuery] int page, AppDbContext context){
            List<Location> allLocations = context.Locations.ToList<Location>();
            Location searchedLocation;

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
                        return NotFound();
                    }

                    jsonObj[0].Lat = jsonObj[0].Lat.Replace('.', ',');
                    jsonObj[0].Lon = jsonObj[0].Lon.Replace('.', ',');

                    searchedLocation = jsonObj[0];

                    allLocations.ForEach(p => {
                        double localDistance = Haversine.Formula(
                            double.Parse(jsonObj[0].Lat), double.Parse(jsonObj[0].Lon),
                            double.Parse(p.Lat.Replace('.', ',')), double.Parse(p.Lon.Replace('.', ',')));
                        p.ChangeDistance(Math.Round(localDistance, 2));
                    });
                }
                else
                {
                    return NotFound();
                }
            }

            List<Location> closerLocations = allLocations
            .OrderBy(p => p.Distance)
            .Skip(page * pageLimit)
            .Take(3)
            .ToList<Location>();
            
            if(closerLocations.Count <= 0){
                return NotFound( new { Message = "All locations has been showed", AllShowed = true });
            }
            
            return Ok(closerLocations);
        }
    }
}
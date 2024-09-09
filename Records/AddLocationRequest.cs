namespace GymProjectBackend.Records;

public record AddLocationRequest(string Display_Name, string Name, string Lat, string Lon, double Distance);
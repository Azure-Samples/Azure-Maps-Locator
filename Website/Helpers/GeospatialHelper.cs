namespace StoreLocator.Helpers;

public static class GeospatialHelper
{
    private const double DegreesToRadians = Math.PI / 180.0;

    public static double CalculateDistanceInKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        // Distance calculation logic using Pythagoras theorem
        var dLat = (latitude2 - latitude1) * DegreesToRadians;
        var dLon = (longitude2 - longitude1) * DegreesToRadians;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(latitude1 * (Math.PI / 180.0)) * Math.Cos(latitude2 * (Math.PI / 180.0)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = 6371 * c; // Earth's radius is approximately 6371 km

        return distance;
    }

    public static bool IsValidLatitude(double latitude)
    {
        // Check if the latitude is within valid range (-90 to 90 degrees)
        return latitude >= -90 && latitude <= 90;
    }

    public static bool IsValidLongitude(double longitude)
    {
        // Check if the longitude is within valid range (-180 to 180 degrees)
        return longitude >= -180 && longitude <= 180;
    }
}
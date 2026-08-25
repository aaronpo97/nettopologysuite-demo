namespace Domain;

public sealed class Landmark
{
    public int LandmarkId { get; init; }
    public string Description { get; init; } = string.Empty;
    public int CityId { get; init; }
    public Coordinates Location { get; init; }

    public double DistanceToCityCentre { get; init; } = -1.0; // Distance to city centre in meters

    // Navigation property
    public City? City { get; set; }
}
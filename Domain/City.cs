namespace Domain;

public sealed class City
{
    public int CityId { get; init; }
    public string Description { get; init; } = string.Empty;
    public Coordinates CityCentre { get; init; }
    public int StateId { get; init; }

    // Navigation property
    public State? State { get; set; }
}
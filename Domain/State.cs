namespace Domain;

public sealed class State
{
    public int StateId { get; init; }
    public string? Description { get; init; }
    public int CountryId { get; init; }

    // Navigation properties
    public Country? Country { get; set; }
    public ICollection<City> Cities { get; init; } = [];
}
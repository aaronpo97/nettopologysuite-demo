namespace Domain;

public sealed class Country
{
    public int CountryId { get; init; }
    public string? Description { get; init; }

    // Navigation property
    public ICollection<State> States { get; init; } = [];
}
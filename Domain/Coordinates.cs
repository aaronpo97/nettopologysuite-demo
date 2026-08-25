namespace Domain;

public readonly record struct Coordinates(double Latitude, double Longitude)
{
   public override string ToString() => $"({Latitude}, {Longitude})";
};


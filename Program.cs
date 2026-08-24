using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class Country
{
    public int CountryId { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public ICollection<State> States { get; set; } = [];
}

public class State
{
    public int StateId { get; set; }
    public string? Description { get; set; }
    public int CountryId { get; set; }

    // Navigation properties
    public Country? Country { get; set; }
    public ICollection<City> Cities { get; set; } = [];
}

public class City
{
    public int CityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Point CityCentre { get; set; } = null!;
    public int StateId { get; set; }

    // Navigation property
    public State? State { get; set; }
}

public class Landmark
{
    public int LandmarkId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CityId { get; set; }
    public Point Location { get; set; } = null!;

    // Navigation property
    public City? City { get; set; }
}

public sealed class PointTypeHandler : SqlMapper.TypeHandler<Point>
{
    private readonly SqlServerBytesReader _reader = new() { IsGeography = true };

    public override Point Parse(object value) => (Point)_reader.Read((byte[])value);

    public override void SetValue(IDbDataParameter parameter, Point? value) =>
        throw new NotSupportedException();
}

public sealed class Repository(string connectionString)
{
    static Repository()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new PointTypeHandler());
    }

    public async Task<List<City>> GetCitiesAsync()
    {
        await using SqlConnection connection = new(connectionString);

        const string sql = """
            SELECT
                c.city_id, c.description, c.state_id, CAST(c.city_centre AS VARBINARY(MAX)) AS city_centre,
                s.state_id, s.description, s.country_id,
                co.country_id, co.description
            FROM city c
            INNER JOIN state s ON c.state_id = s.state_id
            INNER JOIN country co ON s.country_id = co.country_id
            ORDER BY c.city_id;
            """;

        var cities = await connection.QueryAsync<City, State, Country, City>(
            sql,
            (city, state, country) =>
            {
                state.Country = country;
                city.State = state;
                return city;
            },
            splitOn: "state_id,country_id"
        );

        return cities.ToList();
    }

    public async Task<List<Landmark>> GetLandmarksAsync()
    {
        await using SqlConnection connection = new(connectionString);

        const string sql = """
            SELECT
                l.landmark_id, l.description, l.city_id, CAST(l.location AS VARBINARY(MAX)) AS location,
                c.city_id, c.description, c.state_id, CAST(c.city_centre AS VARBINARY(MAX)) AS city_centre
            FROM landmark l
            INNER JOIN city c ON l.city_id = c.city_id
            ORDER BY l.landmark_id;
            """;

        var landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
            sql,
            (landmark, city) =>
            {
                landmark.City = city;
                return landmark;
            },
            splitOn: "city_id"
        );

        return landmarks.ToList();
    }

    public async Task<List<Landmark>> GetLandmarkInRadius(
        double latitude,
        double longitude,
        double radiusInMeters
    )
    {
        await using SqlConnection connection = new(connectionString);

        const string sql = """
            SELECT
                l.landmark_id, l.description, l.city_id, CAST(l.location AS VARBINARY(MAX)) AS location,
                c.city_id, c.description, c.state_id, CAST(c.city_centre AS VARBINARY(MAX)) AS city_centre
            FROM landmark l
            INNER JOIN city c ON l.city_id = c.city_id
            WHERE geography::Point(@Latitude, @Longitude, 4326).STDistance(l.location) <= @RadiusInMeters
            ORDER BY l.landmark_id;
            """;

        var landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
            sql,
            (landmark, city) =>
            {
                landmark.City = city;
                return landmark;
            },
            new
            {
                Latitude = latitude,
                Longitude = longitude,
                RadiusInMeters = radiusInMeters,
            },
            splitOn: "city_id"
        );

        return landmarks.ToList();
    }
}

internal class Program
{
    private static async Task Main(string[] args)
    {
        string connectionString =
            "Server=localhost,1433;Database=locations_db;User Id=sa;Password=YourStrong@Password2026;TrustServerCertificate=True;";

        Repository repository = new Repository(connectionString);

        List<Landmark> landmarksInRadius = await repository.GetLandmarkInRadius(
            40.7128,
            -74.0060,
            5000
        );

        Console.WriteLine("\nLandmarks within 5km of New York City:");
        foreach (var landmark in landmarksInRadius)
        {
            Console.WriteLine(
                $"Landmark: {landmark.Description}, City: {landmark.City?.Description}"
            );
        }
    }
}

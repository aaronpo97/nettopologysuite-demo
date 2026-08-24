using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Spectre.Console;

internal static class EmbeddedSql
{
    public static string Load(string fileName)
    {
        Assembly assembly = typeof(EmbeddedSql).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(fileName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Embedded SQL resource '{fileName}' was not found."
            );
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}

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

    public decimal DistanceToCityCentre { get; set; } = -1.0m; // Distance to city centre in meters

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
    private const int Srid = 4326;

    private static readonly string GetCitiesSql = EmbeddedSql.Load("GetCities.sql");
    private static readonly string GetLandmarksSql = EmbeddedSql.Load("GetLandmarks.sql");
    private static readonly string GetLandmarkInRadiusSql = EmbeddedSql.Load(
        "GetLandmarkInRadius.sql"
    );

    static Repository()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new PointTypeHandler());
    }

    public async Task<List<City>> GetCitiesAsync()
    {
        await using SqlConnection connection = new(connectionString);

        var cities = await connection.QueryAsync<City, State, Country, City>(
            GetCitiesSql,
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

        var landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
            GetLandmarksSql,
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

        var landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
            GetLandmarkInRadiusSql,
            (landmark, city) =>
            {
                landmark.City = city;
                return landmark;
            },
            new
            {
                Latitude = latitude,
                Longitude = longitude,
                Srid,
                RadiusInMeters = radiusInMeters,
            },
            splitOn: "city_id"
        );

        return landmarks.ToList();
    }
}

internal class Program
{
    static decimal ToMeters(decimal distanceInKilometers) => distanceInKilometers * 1000;

    static decimal ToKilometers(decimal distanceInMeters) => distanceInMeters / 1000;

    private static async Task Main()
    {
        string connectionString =
            "Server=localhost,1433;Database=locations_db;User Id=sa;Password=YourStrong@Password2026;TrustServerCertificate=True;";

        Repository repository = new(connectionString);

        List<City> cities = await AnsiConsole
            .Status()
            .StartAsync("Loading cities...", _ => repository.GetCitiesAsync());



        foreach (City city in cities)
        {
            State? state = city.State;
            Country? country = state?.Country;

            double latitude = city.CityCentre.Y;
            double longitude = city.CityCentre.X;

            // get landmarks in 100 km radius of the city centre
            List<Landmark> landmarksInRadius = await repository.GetLandmarkInRadius(
                latitude,
                longitude,
                100 * 1000 // 100 km in meters
            );

            AnsiConsole.Write(
                new Spectre.Console.Rule($"[bold yellow]{city.Description}[/]").LeftJustified()
            );
            AnsiConsole.MarkupLine(
                $"[grey]{state?.Description}, {country?.Description} — ({latitude}, {longitude})[/]"
            );
            AnsiConsole.WriteLine();

            if (landmarksInRadius.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No landmarks found within 100 km.[/]");
            }
            else
            {
                Table table = new Table()
                    .AddColumn(new TableColumn("[italic]Landmark[/]").LeftAligned())
                    .AddColumn(new TableColumn("[italic]Coordinates[/]").LeftAligned())
                    .AddColumn(new TableColumn("[italic]Distance to Centre.[/]").LeftAligned());

                foreach (
                    Landmark landmark in landmarksInRadius.OrderBy(l => l.DistanceToCityCentre)
                )
                {
                    table.AddRow(
                        Markup.Escape(landmark.Description),
                        $"{landmark.Location.Y:F6}, {landmark.Location.X:F6}",
                        $"{ToKilometers(landmark.DistanceToCityCentre):F2} km"
                    );
                }

                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
        }
    }
}

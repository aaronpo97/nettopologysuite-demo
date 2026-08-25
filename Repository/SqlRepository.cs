using Dapper;
using Domain;
using Microsoft.Data.SqlClient;

namespace Repository;

public interface IRepository
{
    Task<List<City>> GetCitiesAsync();
    Task<City?> GetCityByIdAsync(int cityId);
    Task<List<Landmark>> GetLandmarksAsync();

    Task<List<Landmark>> GetLandmarkInRadius(
        double latitude,
        double longitude,
        double radiusInMeters);
}

public sealed class SqlRepository(string connectionString) : IRepository
{
    private const int Srid = 4326;

    private static readonly string GetCitiesSql = EmbeddedSql.Load("GetCities.sql");
    private static readonly string GetCityByIdSql = EmbeddedSql.Load("GetCityById.sql");
    private static readonly string GetLandmarksSql = EmbeddedSql.Load("GetLandmarks.sql");

    private static readonly string GetLandmarkInRadiusSql = EmbeddedSql.Load(
        "GetLandmarkInRadius.sql"
    );

    static SqlRepository()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new CoordinatesTypeHandler());
    }

    public async Task<List<City>> GetCitiesAsync()
    {
        await using SqlConnection connection = new(connectionString);

        IEnumerable<City> cities = await connection.QueryAsync<City, State, Country, City>(
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

    public async Task<City?> GetCityByIdAsync(int cityId)
    {
        await using SqlConnection connection = new(connectionString);

        IEnumerable<City> cities = await connection.QueryAsync<City, State, Country, City>(
            GetCityByIdSql,
            (city, state, country) =>
            {
                state.Country = country;
                city.State = state;
                return city;
            },
            new { CityId = cityId },
            splitOn: "state_id,country_id"
        );

        return cities.SingleOrDefault();
    }

    public async Task<List<Landmark>> GetLandmarksAsync()
    {
        await using SqlConnection connection = new(connectionString);

        IEnumerable<Landmark> landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
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

        IEnumerable<Landmark> landmarks = await connection.QueryAsync<Landmark, City, Landmark>(
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
                RadiusInMeters = radiusInMeters
            },
            splitOn: "city_id"
        );

        return landmarks.ToList();
    }
}
# NetTopologySuite demo

A .NET console application that demonstrates spatial queries against SQL Server's `GEOGRAPHY` type using NetTopologySuite, Dapper, and Spectre.Console.

## Why this exists

SQL Server stores spatial data in a binary `GEOGRAPHY` format that .NET code cannot use directly. This project shows one way to bridge that gap end to end:

- Storing city centres and landmarks as `GEOGRAPHY` points in SQL Server (`Repository/Sql/schema.sql`).
- Querying landmarks within a radius of a point with `STDistance`, computed server-side (`Repository/Sql/GetLandmarkInRadius.sql`).
- Mapping the `GEOGRAPHY` binary column to a plain `Coordinates` struct with `NetTopologySuite.IO.SqlServerBytes`, via a custom Dapper `SqlMapper.TypeHandler<T>` (`Repository/CoordinatesTypeHandler.cs`).
- Presenting the result as an interactive terminal UI with Spectre.Console (`Demo/Program.cs`).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) with Compose, to run the bundled SQL Server 2022 container — or an existing SQL Server instance that supports the `GEOGRAPHY` type
- `sqlcmd`, to load the schema and seed data (bundled inside the SQL Server container used below)

## Getting started

### 1. Start SQL Server

```sh
docker compose up -d
```

This starts a SQL Server 2022 Express container (`sqlserver_container`) on port `1433` with the SA password `YourStrong@Password2026`, defined in `docker-compose.yaml`.

### 2. Create the schema and seed data

`Repository/Sql/schema.sql` drops and recreates the `nettopologysuitedemo` database, then seeds it with countries, states, cities, and landmarks. Run it against the container:

```sh
docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password2026' -C \
  -i /dev/stdin < Repository/Sql/schema.sql
```

### 3. Set the connection string

The app reads the connection string from the `ConnectionStrings__DefaultConnection` environment variable (the double underscore is how `Microsoft.Extensions.Configuration` maps environment variables to configuration keys):

```sh
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=nettopologysuitedemo;User Id=sa;Password=YourStrong@Password2026;TrustServerCertificate=True;"
```

### 4. Run the app

```sh
dotnet run --project Demo
```

The app loads all cities, then repeatedly prompts for a city id and a search radius (in kilometres), and lists the landmarks within that radius ordered by distance from the city centre.

## Project structure

| Project | Contents |
| --- | --- |
| `Demo` | Console entry point and the Spectre.Console terminal UI |
| `Domain` | Plain data types: `City`, `State`, `Country`, `Landmark`, `Coordinates` |
| `Repository` | `SqlRepository` (Dapper queries against SQL Server), embedded `.sql` files, and `CoordinatesTypeHandler`, which converts between SQL Server `GEOGRAPHY` values and `Domain.Coordinates` |

## Building from source

```sh
dotnet build
```

The repository pins [CSharpier](https://csharpier.com/) as a local tool for formatting (`dotnet-tools.json`, `.csharpierc`):

```sh
dotnet tool restore
dotnet csharpier .
```

## License

[MIT](LICENSE)

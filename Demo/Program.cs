using Domain;
using Microsoft.Extensions.Configuration;
using Repository;
using Spectre.Console;

namespace Demo;

public class Program
{
    private static async Task Main()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        IRepository repository = new SqlRepository(connectionString);

        List<City> cities = await AnsiConsole
            .Status()
            .StartAsync("Loading cities...", _ => repository.GetCitiesAsync());

        bool continuePrompt;

        do
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Landmark Search[/]").LeftJustified());


            Table table = new Table().AddColumns(
                "Id",
                "City",
                "State",
                "Country",
                "Latitude",
                "Longitude"
            );
            foreach (City city in cities)
            {
                State? state = city.State;
                Country? country = state?.Country;

                double latitude = city.CityCentre.Latitude;
                double longitude = city.CityCentre.Longitude;

                table.AddRow(
                    $"{city.CityId}",
                    city.Description,
                    state?.Description ?? string.Empty,
                    country?.Description ?? string.Empty,
                    $"{latitude}",
                    $"{longitude}"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            int selectedCityId = AnsiConsole.Ask<int>("Enter the [green]city id[/] to inspect:");
            double radiusInKilometers = AnsiConsole.Ask<double>(
                "Enter the search [green]radius (km)[/]:"
            );
            City? selectedCity = await repository.GetCityByIdAsync(selectedCityId);

            if (selectedCity is null)
            {
                AnsiConsole.MarkupLine($"[red]No city found with id {selectedCityId}.[/]");
                continuePrompt = AnsiConsole.Confirm("Do you want to search for another city?");
                continue;
            }

            List<Landmark> landmarksInRadius = await AnsiConsole
                .Status()
                .StartAsync(
                    "Searching for landmarks...",
                    _ =>
                        repository.GetLandmarkInRadius(
                            selectedCity.CityCentre.Latitude,
                            selectedCity.CityCentre.Longitude,
                            radiusInKilometers * 1000
                        )
                );

            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule($"[bold yellow]{selectedCity.Description}[/] {selectedCity.CityCentre}").LeftJustified()
            );
            AnsiConsole.WriteLine();

            if (landmarksInRadius.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    $"[grey]No landmarks found within {radiusInKilometers} km.[/]"
                );
                continuePrompt = AnsiConsole.Confirm("Do you want to search for another city?");
                continue;
            }

            AnsiConsole.MarkupLine(
                $"[grey]{landmarksInRadius.Count} landmark(s) found within {radiusInKilometers} km.[/]"
            );

            Table landmarksTable = new Table()
                .AddColumn(new TableColumn("[italic]Landmark[/]").LeftAligned())
                .AddColumn(new TableColumn("[italic]Coordinates[/]").LeftAligned())
                .AddColumn(new TableColumn("[italic]Distance to Centre[/]").LeftAligned());

            foreach (Landmark landmark in landmarksInRadius.OrderBy(l => l.DistanceToCityCentre))
            {
                landmarksTable.AddRow(
                    Markup.Escape(landmark.Description),
                    $"{landmark.Location.Latitude:F6}, {landmark.Location.Longitude:F6}",
                    $"{landmark.DistanceToCityCentre / 1000:F2} km"
                );
            }

            AnsiConsole.Write(landmarksTable);

            continuePrompt = AnsiConsole.Confirm("Do you want to search for another city?");
        } while (continuePrompt);
    }
}

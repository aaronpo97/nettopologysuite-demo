using System.Reflection;

namespace Repository;

internal static class EmbeddedSql
{
    public static string Load(string fileName)
    {
        Assembly assembly = typeof(EmbeddedSql).Assembly;

        string? resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith($".{fileName}", StringComparison.Ordinal));

        if (resourceName is null)
            throw new InvalidOperationException(
                $"Embedded SQL resource '{fileName}' was not found."
            );

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
            throw new InvalidOperationException(
                $"Embedded SQL resource '{fileName}' was not found."
            );

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
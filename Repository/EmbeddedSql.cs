using System.Reflection;

namespace Repository;

internal static class EmbeddedSql
{
    public static string Load(string fileName)
    {
        Assembly assembly = typeof(EmbeddedSql).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(fileName);

        if (stream is null)
            throw new InvalidOperationException(
                $"Embedded SQL resource '{fileName}' was not found."
            );

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
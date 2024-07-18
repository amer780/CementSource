using System.IO;
using System.Reflection;
using MelonLoader;

namespace CementGB.Mod.Utilities;

public static class FileUtilities
{
    public static string? ReadEmbeddedText(Assembly assembly, string resourceName)
    {
        assembly ??= Assembly.GetCallingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Melon<Mod>.Logger.Warning($"Assembly {assembly.FullName} failed to find embedded resource {resourceName} within. Ensure the resource name is correct.");
            return null;
        }
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
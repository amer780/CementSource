using System.IO;

public static class DirectoryExtender
{
    public static void DeleteFilesInDirectory(string path)
    {
        foreach (string sub in Directory.GetDirectories(path))
        {
            DeleteFilesInDirectory(sub);
        }

        foreach (string file in Directory.EnumerateFiles(path))
        {
            File.Delete(file);
        }

        Directory.Delete(path);
    }
}
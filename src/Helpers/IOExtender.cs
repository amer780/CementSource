namespace CementTools.Helpers;

public static class IOExtender
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

    public static string GetFileName(string path)
        {
            string fileName = "";
            foreach (char c in path)
            {
                if (c == '\\' || c == '/')
                {
                    fileName = "";
                }
                else
                {
                    fileName += c;
                }
            }

            return fileName;
        }
}
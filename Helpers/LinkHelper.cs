using CementTools;

public static class LinkHelper
{
    public static bool IsLinkToMod(string link)
    {
        string[] split = link.Split('.');
        return split[split.Length - 1] == Cement.MOD_FILE_EXTENSION;
    }

    public static string GetNameFromLink(string link)
    {
        string[] split = link.Split('/');
        return ToUsableName(split[split.Length - 1]);
    }

    public static bool IsLink(string possibleLink)
    {
        if (possibleLink.IndexOf("https://") == 0)
        {
            return true;
        }
        return false;
    }

    const string BANNED = "/<>:\"\\|?*";
    public static string ToUsableName(string name)
    {
        string newName = name;
        foreach (char c in BANNED)
        {
            newName = newName.Replace(c, '_');
        }

        newName = URLManager.URLToNormal(newName);
        return newName;
    }
}
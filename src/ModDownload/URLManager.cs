using System.Collections.Generic;

class URLManager
{
    private static readonly Dictionary<string, char> unsupportedUrlEscapeCodes = new Dictionary<string, char>()
{
    { "%3C", '<' },
    { "%3E", '>' },
    { "%22", '"' },
    { "%23", '#' },
    { "%25", '%' },
    { "%7B", '{' },
    { "%5C", '\\'},
    { "%5E", '^' },
    { "%7E", '~' },
    { "%5B", '[' },
    { "%5D", ']' },
    { "%60", '`' },
    { "%2C", ',' },
    { "%3A", ':' },
    { "%3B", ';' },
    { "%3F", '?' },
    { "%2F", '/' },
    { "%40", '@' },
    { "%3D", '=' },
    { "%26", '&' },
    { "%2B", '+' },
    { "%24", '$' },
    { "%2A", '*' },
    { "%5F", '_' },
    { "%2D", '-' },
    { "%21", '!' },
    { "%28", '(' },
    { "%29", ')' },
    { "%7D", '}' },
    { "%7C", '|' },
    { "%20", ' ' },
};

    // this function converts the url escape codes to their proper ascii characters
    public static string URLToNormal(string normal)
    {
        foreach (string code in unsupportedUrlEscapeCodes.Keys)
        {
            normal = normal.Replace(code, unsupportedUrlEscapeCodes[code].ToString());
        }

        return normal;
    }
}
using System.IO;
using System.Collections.Generic;
using CementTools;

class ModFile
{
    Dictionary<string,string> _values = new Dictionary<string, string>();
    string _path;

    public ModFile(string path)
    {
        _path = path;
        foreach (string line in File.ReadLines(path))
        {
            if (line == "")
            {
                continue;
            }
            string[] splitLine = line.Split('=');
            if (splitLine.Length < 2)
            {
                throw new System.Exception($"Incorrect format for {Cement.MOD_FILE_EXTENSION} file.");
            }
            string key = splitLine[0];
            string value = splitLine[1];
            _values[key] = value;
        }
    }

    public string GetValue(string key)
    {
        if (!_values.ContainsKey(key))
        {
            return null;
        }

        return _values[key];
    }

    public void SetValue(string key, string value)
    {
        _values[key] = value;
    }

    public void UpdateFile()
    {
        string fileContents = "";
        foreach (string key in _values.Keys)
        {
            fileContents += $"{key}={_values[key]}\n";
        }

        File.WriteAllText(_path, fileContents);
    }
}
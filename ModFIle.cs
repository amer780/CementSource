using System.IO;
using System.Collections.Generic;
using CementTools;

public class ModFile
{
    Dictionary<string,string> _values = new Dictionary<string, string>();
    string _path;

    public ModFile(string path)
    {
        _path = path;
        Reload(false);
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

    public string GetString(string key)
    {
        return GetValue(key);
    }

    public void SetString(string key, string value)
    {
        SetValue(key, value);
    }

    public float GetFloat(string key)
    {
        string b = GetValue(key);
        return float.Parse(b);
    }

    public void SetFloat(string key, float value)
    {
        SetValue(key, value.ToString());
    }

    public int GetInt(string key)
    {
        string b = GetValue(key);
        return int.Parse(b);
    }

    public void SetInt(string key, int value)
    {
        SetValue(key, value.ToString());
    }

    public bool GetBool(string key)
    {
        string b = GetValue(key);
        return b == "true";
    }

    public void SetBool(string key, bool value)
    {
        if (value)
        {
            SetValue(key, "true");
        }
        else
        {
            SetValue(key, "false");
        }
    }

    public void UpdateFile()
    {
        StringWriter writer = new StringWriter();
        Dictionary<string, string>.KeyCollection.Enumerator enumerator = _values.Keys.GetEnumerator();
        for (int i = 0; i < _values.Count; i++)
        {
            enumerator.MoveNext();
            string key = enumerator.Current;
            writer.Write(key + "=" + _values[key]);

            if (i != _values.Count - 1)
            {
                writer.WriteLine();
            }
        }

        File.WriteAllText(_path, writer.ToString());
    }

    public void Reload(bool clearExistingValues = true)
    {
        if (clearExistingValues)
        {
            _values.Clear();
        }
        
        foreach (string line in File.ReadLines(_path))
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
            string value = "";
            for (int i = 1; i < splitLine.Length; i++)
            {
                value += splitLine[i];
                if (i + 1 != splitLine.Length)
                {
                    value += "=";
                }
            }
            _values[key] = value;
        }
    }
}
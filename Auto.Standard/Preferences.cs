using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Auto
{
    public class Preferences
    {
        public static readonly Preferences Tmp    = new Preferences("TmpPrefs.json");
        public static readonly Preferences Local  = new Preferences("LocalPrefs.json");
        public static readonly Preferences Global = new Preferences(Home + "/Auto/GlobalPrefs.json");

        private string                     _name;
        private Dictionary<string, object> _dict;

        private JsonSerializerOptions _settings;
        private JsonSerializerOptions Settings
        {
            get
            {
                if(_settings == null)
                {
                    _settings = new JsonSerializerOptions();
                    _settings.WriteIndented = true;
                    _settings.Converters.Add(new DirectoryInfoConverter());
                    _settings.Converters.Add(new FileInfoConverter());
                    _settings.Converters.Add(new UnknownPathConverter());
                }
                return _settings;
            }
        }

        public Preferences(string name)
        {
            //Assembly.GetExecutingAssembly().Location
            _name = name;
            if(File.Exists(_name))
            {
                var str = File.ReadAllText(_name);
                _dict = JsonSerializer.Deserialize<Dictionary<string, object>>(str, Settings);
            }
            else
            {
                _dict = new Dictionary<string, object>();
                SaveToDisk();
            }
        }

        private void SaveToDisk()
        {
            var str = JsonSerializer.Serialize(_dict, Settings);

            var fullPath = Path.GetFullPath(_name);
            var directory = Path.GetDirectoryName(fullPath);
            if(!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            File.WriteAllText(fullPath, str);
        }

        void SaveImp<T>(string key, T value)
        {
            _dict[key] = value;
            SaveToDisk();
        }

        bool LoadImp<T>(string key, out T result, T defaultValue)
        {
            defaultValue = default;
            if(_dict.TryGetValue(key, out var value))
            {
                if(value == null)
                {
                    result = default;
                    return true;
                }
                else if(value is T tvalue)
                {
                    result = tvalue;
                    return true;
                }
            }

            result = defaultValue;
            return false;
        }
        public void Save<T>(string key, T value)
        {
            SaveImp<T>(key, value);
        }
        public bool Load<T>(string key, out T value)
        {
            return LoadImp<T>(key, out value, default);
        }
        public void Load<T>(string key, out T value, T defaultValue)
        {
            LoadImp<T>(key, out value, defaultValue);
        }
        public string LoadStr(string key, string defaultValue = default)
        {
            if(!LoadImp(key, out string value, defaultValue))
            {
                value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            }

            return value;
        }
        public int LoadInt(string key, int defaultValue = default)
        {
            LoadImp(key, out int value, defaultValue);
            return value;
        }

        // public static string Home => (Environment.OSVersion.Platform == PlatformID.Unix ||
        //                               Environment.OSVersion.Platform == PlatformID.MacOSX)
        //     ? Environment.GetEnvironmentVariable("HOME")
        //     : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Drawing;

namespace AppChooser
{
    public class AppConfig
    {
        public string ExePath;
        public string Parameters;
        public string Files;
        public string DisplayName;
        [JsonIgnore]
        public string OldDisplayName; // Used to keep InputText in focus when DisplayName is changed
        
        public AppConfig(string exePath, string files, string displayName, string parameters = "{filename}")
        {
            ExePath = exePath;
            Parameters = parameters;
            Files = files;
            if (string.IsNullOrEmpty(displayName))
            {
                DisplayName = GetDisplayName(exePath);
            }
            else
            {
                DisplayName = displayName;
            }
            OldDisplayName = DisplayName;
        }

        public void Launch(string filename)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = ExePath,
                Arguments = "\"" + Parameters.Replace("{filename}", filename) + "\""
            };
            Process.Start(startInfo);
        }

        public static string GetDisplayName(string exePath)
        {
            return Path.GetFileNameWithoutExtension(exePath).FirstCharToUpper();
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

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
            string parameters = Parameters.Replace("{filename}",filename);
            Process.Start(ExePath, parameters);
        }

        public static string GetDisplayName(string exePath)
        {
            return Path.GetFileNameWithoutExtension(exePath).FirstCharToUpper();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Vulkan.Xlib;

namespace AppChooser
{
    public static class Settings
    {
        public static string SettingsPath = AppContext.BaseDirectory + "Settings.txt"; // Don't want to use appdata
        public static Vector2 WindowPosition = new Vector2(50, 50);
        public static Vector2 WindowSize = new Vector2(1920, 1080);

        public static Dictionary<string, AppConfig> AppsByExtension;

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsPath)) File.Create(SettingsPath);

            foreach (string s in File.ReadAllLines(SettingsPath))
            {
                if (s.StartsWith("WindowPosition: "))
                {
                    string[] vec = s.Substring(16).Split(',');
                    WindowPosition = new Vector2(float.Parse(vec[0]), float.Parse(vec[1]));
                }
                else if (s.StartsWith("WindowSize: "))
                {
                    string[] vec = s.Substring(12).Split(',');
                    WindowSize = new Vector2(float.Parse(vec[0]), float.Parse(vec[1]));
                }
            }
        }

        public static void SaveSettings()
        {
            WindowSize.X = Program.Window.Width; WindowSize.Y = Program.Window.Height;
            WindowPosition.X = Program.Window.X; WindowPosition.Y = Program.Window.Y;

            File.Delete(SettingsPath);
            File.WriteAllText(SettingsPath, GetSaveText());
        }

        public static string GetSaveText()
        {
            return $"WindowPosition: {WindowPosition.X},{WindowPosition.Y}\nWindowSize: {WindowSize.X},{WindowSize.Y}";
        }
    }
}

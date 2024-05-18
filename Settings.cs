using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Vulkan.Xlib;
using System.Text.Json;
using System.Linq;
using System.Reflection;

namespace AppChooser
{
    public static class Settings
    {
        public static string SettingsRoot = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/AppChooser/");
        public static string SettingsPath => SettingsRoot + "Settings.txt";
        public static string ExecutablesPath => SettingsRoot + "Executables.json";
        public static string ExecutablesBackupPath => SettingsRoot + "Executables_broken.json";

        [Save]
        public static Vector2 WindowPosition = new Vector2(50, 50);
        [Save]
        public static Vector2 WindowSize = new Vector2(1920, 1080);
        [Save]
        public static bool KeepPosition = true;
        [Save]
        public static bool KeepSize = true;
        
        [Save]
        public static float FontSize = 1f;
        [Save]
        public static bool DontLaunchIfOnlyOneAppAvaible = true;

        public static List<AppConfig> Apps = new();

        public static void LoadSettings(bool recursed = false)
        {
            Directory.CreateDirectory(SettingsRoot);
            if (!File.Exists(SettingsPath)) File.Create(SettingsPath);
            if (!File.Exists(ExecutablesPath)) File.Create(ExecutablesPath);

            LoadAllData();


            string jsonText = File.ReadAllText(ExecutablesPath);
            if (!string.IsNullOrEmpty(jsonText)) // Probably I should do some better check.
            {
                var options = new JsonSerializerOptions();
                options.IncludeFields = true;
                try
                {
                    Apps = JsonSerializer.Deserialize<List<AppConfig>>(jsonText, options) ?? Apps;
                }
                catch (Exception e)
                {
                    File.Move(ExecutablesPath, ExecutablesBackupPath);
                    Console.WriteLine("Could not load Executables.json, error was catched. Executables.json was renamed to Executables_broken.json .");
                    Console.WriteLine(e);
                }
            }
        }

        public static void SaveSettings()
        {
            WindowSize.X = Program.Window.Width; WindowSize.Y = Program.Window.Height;
            WindowPosition.X = Program.Window.X; WindowPosition.Y = Program.Window.Y;

            SaveAllData();

            var options = new JsonSerializerOptions();
            options.IncludeFields = true;
            File.WriteAllText(ExecutablesPath, JsonSerializer.Serialize(Apps, options));
        }

        #region FileSystem

        private static void LoadAllData()
        {
            List<string> saveData = File.ReadAllLines(Settings.SettingsPath).ToList();

            foreach (string data in saveData)
            {
                string[] splitData = data.Split(':');

                var type = Type.GetType(splitData[0]);
                var field = type?.GetField(splitData[1]);
                if (field is null) continue;

                try
                {

                    //------------------------------------------------------------------------------------------
                    //THIS IS WHERE DATA IS PARSED FOR LOADING. EDIT THIS TO ADD SUPPORT FOR MORE TYPES OF VALUE

                    if (field.FieldType == typeof(string))
                    {
                        field.SetValue(null, splitData[2]);
                        continue;
                    }

                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(null, int.Parse(splitData[2]));
                        continue;
                    }

                    if (field.FieldType == typeof(float))
                    {
                        field.SetValue(null, float.Parse(splitData[2]));
                        continue;
                    }

                    if (field.FieldType == typeof(double))
                    {
                        field.SetValue(null, double.Parse(splitData[2]));
                        continue;
                    }

                    if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(null, bool.Parse(splitData[2]));
                        continue;
                    }

                    if (field.FieldType == typeof(Vector2))
                    {
                        string[] splitSplitData = splitData[2].Split('.');
                        float x = float.Parse(splitSplitData[0]);
                        float y = float.Parse(splitSplitData[1]);
                        field.SetValue(null, new Vector2(x, y));
                    }

                    //------------------------------------------------------------------------------------------
                }
                catch
                {
                    Console.WriteLine("Failed to load setting " + splitData[1]);
                }
            }

        }

        private static void SaveAllData()
        {
            List<string> saveData = new List<string>();

            foreach (var field in GetAllSettings())
            {

                string data = field.DeclaringType + ":" + field.Name + ":";

                //------------------------------------------------------------------------------------------
                //THIS IS WHERE DATA IS PARSED FOR SAVING. EDIT THIS TO ADD SUPPORT FOR MORE TYPES OF VALUE

                if (field.FieldType == typeof(string))
                {
                    data += (string)field.GetValue(null);
                    saveData.Add(data);
                    continue;
                }

                if (field.FieldType == typeof(int))
                {
                    data += (int)field.GetValue(null);
                    saveData.Add(data);
                    continue;
                }

                if (field.FieldType == typeof(float))
                {
                    data += (float)field.GetValue(null);
                    saveData.Add(data);
                    continue;
                }

                if (field.FieldType == typeof(double))
                {
                    data += (double)field.GetValue(null);
                    saveData.Add(data);
                    continue;
                }

                if (field.FieldType == typeof(bool))
                {
                    data += (bool)field.GetValue(null);
                    saveData.Add(data);
                    continue;
                }

                if (field.FieldType == typeof(Vector2))
                {
                    var vec = (Vector2)field.GetValue(null);
                    data += vec.X + "." + vec.Y;
                    saveData.Add(data);
                    continue;
                }

                //------------------------------------------------------------------------------------------
            }

            File.WriteAllLines(Settings.SettingsPath, saveData);
        }

        private static IEnumerable<FieldInfo> GetAllSettings()
        {
            return Assembly.GetExecutingAssembly().GetTypes().SelectMany(x => x.GetFields()).Where(x =>
                x.GetCustomAttributes(typeof(SaveAttribute), false).FirstOrDefault() != null);
        }

        #endregion
    }
}

using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.IO;
using System;
using ImGuiNET;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Microsoft.Win32;


namespace AppChooser
{
    public static class Program
    {
        public static Sdl2Window Window;
        private static GraphicsDevice gd;
        private static CommandList cl;
        private static ImGuiController Controller;

        public static bool openingFile = false;

        public static string file;
        
        private static Vector3 _clearColor = new Vector3(24f / 255f, 24f / 255f, 24f / 255f);

        public static ImGuiIOPtr io;
        public static ImGuiViewportPtr viewport;

        public enum Menu { Main, Settings, FilesAssociations, HelpAndAbout}
        public static Menu CurrentMenu = Menu.Main;

        public static string NewAppConfigPath = "";
        public static string NewAppDisplayName = "";
        public static string NewAppError = "";

        public static List<AppConfig> AvaibleApps = new();
        public static List<AppConfig> OtherApps = new();

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                bool loadSettings = true;
                for (int i = 0; i < args.Length; i++)
                {
                    if (i == 0 && File.Exists(args[i]))
                    {
                        file = args[i];
                        openingFile = true;
                    }

                    if (args[i] == "--default-settings")
                    {
                        loadSettings = false;
                    }
                }

                if (loadSettings)
                {
                    Settings.LoadSettings();
                }

                if (openingFile)
                {
                    UpdateAvaibleApps();
                    if (AvaibleApps.Count == 1)
                    {
                        AvaibleApps[0].Launch(file);
                        Environment.Exit(0);
                    }
                }
                

                int x = 50;
                int y = 50;
                int width = 1080;
                int height = 720;
                if (Settings.KeepPosition)
                {
                    x = Math.Max((int)Settings.WindowPosition.X, 0);
                    y = Math.Max((int)Settings.WindowPosition.Y,0);
                }
                if (Settings.KeepSize)
                {
                    width = Math.Max((int)Settings.WindowSize.X,100); //fool-proof
                    height = Math.Max((int)Settings.WindowSize.Y,100);
                }

                // Create window, GraphicsDevice, and all resources necessary for the demo.
                VeldridStartup.CreateWindowAndGraphicsDevice(
                    new WindowCreateInfo(x,y,width,height, WindowState.Normal, "App Chooser"),
                    new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                    out Window,
                    out gd);
                Window.Resized += () =>
                {
                    gd.MainSwapchain.Resize((uint)Window.Width, (uint)Window.Height);
                    Controller.WindowResized(Window.Width, Window.Height);
                };
                cl = gd.ResourceFactory.CreateCommandList();
                Controller = new ImGuiController(gd, gd.MainSwapchain.Framebuffer.OutputDescription, Window.Width, Window.Height);

                io = ImGui.GetIO();
                viewport = ImGui.GetMainViewport();
                io.FontGlobalScale = Settings.FontSize;
                io.WantCaptureKeyboard = false;

                var stopwatch = Stopwatch.StartNew();
                float deltaTime = 0f;
                // Main application loop
                while (Window.Exists)
                {
                    deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                    stopwatch.Restart();
                    InputSnapshot snapshot = Window.PumpEvents();
                    if (!Window.Exists) { break; }
                    Controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                    Update();

                    cl.Begin();
                    cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
                    cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                    Controller.Render(gd, cl);
                    cl.End();
                    gd.SubmitCommands(cl);
                    gd.SwapBuffers(gd.MainSwapchain);
                }

                Settings.SaveSettings();

                // Clean up Veldrid resources
                gd.WaitForIdle();
                Controller.Dispose();
                cl.Dispose();
                gd.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                CrashHandler.Crash(e);
            }
        }

        private static unsafe void Update()
        {
            if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.O))
            {
                OpenFile();
            }

            //ImGui.ShowDemoWindow();
            ShowMainWindow();
            ShowTopBar();
        }

        public static void ShowTopBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open", "Ctrl+O"))
                    {
                        OpenFile();
                    }

                    if (ImGui.BeginMenu("Menu"))
                    {
                        if (ImGui.MenuItem("Main"))
                        {
                            UpdateAvaibleApps();
                            CurrentMenu = Menu.Main;
                        }

                        if (ImGui.MenuItem("Settings"))
                        {
                            CurrentMenu = Menu.Settings;
                        }
                        if (ImGui.MenuItem("Files Associations"))
                        {
                            CurrentMenu = Menu.FilesAssociations;
                        }
                        if (ImGui.MenuItem("Help/About"))
                        {
                            CurrentMenu = Menu.HelpAndAbout;
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Quit", "Alt+F4"))
                    {
                        Window.Close();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }

        public static void ShowMainWindow()
        {
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.Begin("App Chooser", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration);

            if (CurrentMenu == Menu.Main)
            {
                if (openingFile) // Open file
                {
                    if (AvaibleApps.Count == 0) UpdateAvaibleApps(); //just in case
                    ImGui.Text($"Opening {file}");
                    foreach (AppConfig config in AvaibleApps)
                    {
                        ImGui.PushID(config.DisplayName);

                        if (ImGui.Button($"{config.DisplayName}\n{config.ExePath} {config.Parameters}", new Vector2(Window.Width, io.FontGlobalScale * 30)))
                        {
                            config.Launch(file);
                            Environment.Exit(0);
                        }

                        ImGui.NewLine();
                        ImGui.PopID();
                    }

                    if (OtherApps.Count > 0)
                    {
                        ImGui.NewLine();
                        ImGui.Text("Other Apps:");

                        foreach (AppConfig config in OtherApps)
                        {
                            ImGui.PushID(config.DisplayName);

                            if (ImGui.Button($"{config.DisplayName}\n{config.ExePath} {config.Parameters}", new Vector2(Window.Width, io.FontGlobalScale * 30)))
                            {
                                config.Launch(file);
                                Environment.Exit(0);
                            }

                            ImGui.NewLine();
                            ImGui.PopID();
                        }
                    }
                }
                else // Edit settings..?
                {
                    ImGui.Text("No file selected ^^");
                }
            }


            else if (CurrentMenu == Menu.Settings)
            {
                if (ImGui.BeginTabBar("Settings"))
                {
                    if (ImGui.BeginTabItem("General"))
                    {
                        if (ImGui.InputFloat("Font Size", ref Settings.FontSize))
                        {
                            io.FontGlobalScale = Settings.FontSize;
                        }
                        ImGui.Checkbox("Don't open window if only one app specified for that extension/file", ref Settings.DontLaunchIfOnlyOneAppAvaible);

                        if (ImGui.Button("Add this app to right-click menu"))
                        {
                            RegistryKey k1 = Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("Background").OpenSubKey("shell", true);
                            RegistryKey k2 = k1.CreateSubKey("AppChooser", true);
                            RegistryKey k3 = k2.CreateSubKey("Command", true);
                            k3.SetValue("", Application.ExecutablePath);
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Saving/Loading"))
                    {
                        ImGui.Checkbox("Keep window position on launch", ref Settings.KeepPosition);
                        ImGui.Checkbox("Keep window size on launch", ref Settings.KeepSize);
                        if (ImGui.InputFloat2("Window position", ref Settings.WindowPosition))
                        {
                            Window.X = (int)Settings.WindowPosition.X;
                            Window.Y = (int)Settings.WindowPosition.Y;
                        }
                        if (ImGui.InputFloat2("Window size", ref Settings.WindowSize))
                        {
                            Window.Width = (int)Settings.WindowSize.X;
                            Window.Height = (int)Settings.WindowSize.Y;
                        }
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }


            else if (CurrentMenu == Menu.FilesAssociations)
            {
                AppConfig ToRemove = null;
                foreach (AppConfig config in Settings.Apps)
                {
                    ImGui.PushID(config.OldDisplayName);
                    ImGui.SeparatorText(config.DisplayName);
                    if (ImGui.InputText($"Display name", ref config.DisplayName, 200) && ImGui.IsKeyPressed(ImGuiKey.Enter))
                    {
                        config.OldDisplayName = config.DisplayName;
                    }
                    ImGui.InputText($"App Path", ref config.ExePath, 200);
                    ImGui.InputText($"Parameters", ref config.Parameters, 200);
                    ImGui.InputText($"Extension/File name", ref config.Files, 300);
                    if (ImGui.Button("Remove"))
                    {
                        ToRemove = config;
                    }
                    ImGui.PopID();
                }
                if (ToRemove != null)
                {
                    Settings.Apps.Remove(ToRemove);
                }


                ImGui.SeparatorText("New");
                ImGui.InputText("App's path", ref NewAppConfigPath, 200);
                ImGui.InputText("App's display name", ref NewAppDisplayName, 200);
                if (ImGui.Button("New app"))
                {
                    bool add = true;

                    foreach (AppConfig config in Settings.Apps)
                    {
                        if (config.DisplayName == (string.IsNullOrEmpty(NewAppDisplayName) ? AppConfig.GetDisplayName(NewAppConfigPath) : NewAppDisplayName))
                        {
                            add = false;
                            NewAppError = "App with same display name already exists.\nSorry, but you can't have two apps with same display name for technical reasons ^^'";
                            break;
                        }
                    }

                    if (add)
                    {
                        Settings.Apps.Add(new AppConfig(NewAppConfigPath, "", NewAppDisplayName));
                        NewAppConfigPath = "";
                        NewAppDisplayName = "";
                        NewAppError = "";
                    }
                }
                
                if (!string.IsNullOrEmpty(NewAppError))
                {
                    ImGui.NewLine();
                    ImGui.TextColored(new Vector4(1f,19f/255f,19f/255f, 1f), NewAppError);
                }
            }


            else if (CurrentMenu == Menu.HelpAndAbout)
            {
                if (ImGui.BeginTabBar("HelpAndAbout"))
                {
                    if (ImGui.BeginTabItem("Help"))
                    {
                        ImGui.Text("I started making this app because windows' \"Open with...\" annoyed me one day :)");
                        ImGui.Text("Just go to \"File>Menu>Files Associations\" and create there new app, then open file with this app, and select app you created.");
                        ImGui.Text("You can associate app with multiply extensions by separating them with commands, ex. \".txt,.json\"");
                        ImGui.Text("Contact me if you know how to improve help tab ^^'");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("About"))
                    {
                        ImGui.Text("This app keeps settings in \"%localappdata%/AppChooser/\".");
                        ImGui.Text("You can copypaste files from there if you need to send them to other pc.");
                        ImGui.Text("And you can delete them when uninstalling app (tho they're really small)");
                        ImGui.NewLine();
                        ImGui.Text("App by Dzhake, MIT license, site: https://github.com/Dzhake/AppChooser");
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }


            ImGui.End();
        }


        public static List<AppConfig> GetAppsForExtension(string extension, out List<AppConfig> other)
        {
            List<AppConfig> apps = new List<AppConfig>();
            other = new();

            foreach (AppConfig config in Settings.Apps)
            {
                string[] exts = config.Files.Split(',');
                if (exts.Contains(extension))
                {
                    apps.Add(config);
                }
                else
                {
                    other.Add(config);
                }
            }

            return apps;
        }



        public static void OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    openingFile = true;
                    file = openFileDialog.FileName;
                }
            }
            UpdateAvaibleApps();
        }

        public static void UpdateAvaibleApps()
        {
            AvaibleApps = GetAppsForExtension(Path.GetExtension(file), out OtherApps);
        }
    }
}

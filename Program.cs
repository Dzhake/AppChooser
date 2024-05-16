using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.IO;
using Veldrid.MetalBindings;
using System;
using ImGuiNET;
using System.Windows.Forms;


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
        public static ImFontPtr Renogare;

        // UI state
        private static Vector3 _clearColor = new Vector3(24f / 255f, 24f / 255f, 24f / 255f);

        public static ImGuiIOPtr io;

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

                // Create window, GraphicsDevice, and all resources necessary for the demo.
                VeldridStartup.CreateWindowAndGraphicsDevice(
                    new WindowCreateInfo((int)Settings.WindowPosition.X, (int)Settings.WindowPosition.Y, (int)Settings.WindowSize.X, (int)Settings.WindowSize.Y, WindowState.Normal, "App Chooser"),
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

                //Renogare = io.Fonts.AddFontFromFileTTF(Path.Join(AppContext.BaseDirectory, "Resources", "Renogare.ttf"), 20, null, io.Fonts.GetGlyphRangesDefault());
                io.FontGlobalScale = 4f;

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

                    SubmitUI();

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

        private static unsafe void SubmitUI()
        {
            ShowTopBar();
            //ImGui.ShowDemoWindow();
            ShowMainWindow();
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
            ImGui.SetNextWindowSize(new Vector2(io.DisplaySize.X,io.DisplaySize.Y - 60));
            ImGui.SetNextWindowPos(new Vector2(0, 60));
            ImGui.Begin("App Chooser", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration);

            ImGui.PushFont(Renogare);
            if (openingFile) // Open file
            {
                ImGui.Text($"Opening {file}");
            }
            else // Edit settings..?
            {
                ImGui.Text("No file selected ^^");
            }
            
            ImGui.PopFont();
            ImGui.End();
        }



        public static void OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    openingFile = true;
                    file = openFileDialog.FileName;
                }
            }
        }
    }
}

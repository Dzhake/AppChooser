using System;
using System.IO;

namespace AppChooser
{
    public static class CrashHandler
    {
        public static string ErrorLogPath = AppContext.BaseDirectory + "error_log.txt";
        public static void Crash(Exception e)
        {
            File.WriteAllText(ErrorLogPath, e.Message + "\n" + e.StackTrace);
        }
    }
}

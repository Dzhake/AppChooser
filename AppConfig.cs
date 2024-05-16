using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppChooser
{
    public class AppConfig
    {
        public string ExePath;
        public string Parameters;
        
        public AppConfig(string exePath, string parameters = "")
        {
            ExePath = exePath;
            Parameters = parameters;
        }

        public void Launch(string fileName)
        {
            Process.Start(ExePath,fileName + Parameters);
        }
    }
}

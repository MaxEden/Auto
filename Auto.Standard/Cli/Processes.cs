using System;
using System.Diagnostics;

namespace Auto
{
    public static class Processes
    {
        public static Process Start(Logger logger, string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                logger.Info?.Invoke($"Precess start failed. Executable {path} not found");
                return null;
            }

            Process process = null;
            if(path.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                process = CLI.RunInBackground(logger,"dotnet", path);
            }
            else
            {
                process = CLI.RunInBackground(logger, path);
            }

            Preferences.Tmp.Save("ProcId:" + path, process.Id);

            return process;
        }

        public static void StopLast(Logger logger, string path)
        {
            int id = Preferences.Tmp.LoadInt("ProcId:" + path);
            if(id <= 0) return;

            try
            {
                var proc = Process.GetProcessById(id);
                proc.Kill();
            }
            catch
            {
                logger.Info?.Invoke($"Server with id:{id} not found.");
            }
        }
    }
}
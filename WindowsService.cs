using System.Diagnostics;

namespace Dementor
{
    public class WindowsService : System.ServiceProcess.ServiceBase
    {

        private void Log(string logMessage)
        {
            //Directory.CreateDirectory(Path.GetDirectoryName(_logFileLocation));
            //File.AppendAllText(_logFileLocation, DateTime.UtcNow.ToString() + " : " + logMessage + Environment.NewLine);
        }

        protected override void OnStart(string[] args)
        {
            Log("Starting");
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            Log("Stopping");
            base.OnStop();
        }

        protected override void OnPause()
        {
            Log("Pausing");
            base.OnPause();
        }
    }
}

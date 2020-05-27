using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Timers;

namespace Dementor
{
    public class ProcessMonitor
    {
        /// <summary>
        /// Time between process snapshots in seconds
        /// </summary>
        private int interval;
        private Timer timer;
        private BlackList blackList;

        public ProcessMonitor(int interval = 10)
        {
            this.interval = interval;
            this.blackList = new BlackList();
        }

        public void Start()
        {
            timer = new Timer(interval*1000);
            timer.Elapsed += new ElapsedEventHandler(Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        private void Elapsed(object obj, ElapsedEventArgs args)
        {
            var p = Process.GetProcesses();
        }

        public void Stop()
        {
            if (timer == null)
                return;
            timer.Enabled = false;
            timer.Stop();
        }
    }
}

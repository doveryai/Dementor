using System.Net;
using System.Diagnostics;
using System.Timers;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

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

            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                using (var data = client.OpenRead("https://raw.githubusercontent.com/doveryai/Dementor/master/blacklist.json"))
                {
                    using (var reader = new StreamReader(data))
                    {
                        this.blackList = JsonConvert.DeserializeObject<BlackList>(reader.ReadToEnd());
                    }
                }
            }
        }

        public void Start()
        {
            timer = new Timer(interval * 1000);
            timer.Elapsed += new ElapsedEventHandler(Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        private void Elapsed(object obj, ElapsedEventArgs args)
        {
            var processes = Process.GetProcesses();

            //just do desktop apps for now
            foreach (var a in blackList.Apps)
            {
                foreach (var p in processes.Where(x => x.ProcessName.ToLower().Equals(a.Name.ToLower())))
                {
                    if (!p.CloseMainWindow())
                        p.Kill();
                }
            }
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

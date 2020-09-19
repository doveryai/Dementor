using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dementor
{
    public class ProcessMonitor
    {
        private BlackList blackList;
        private DementorSettings appsettings;
        private readonly ILogger<Worker> logger;

        public ProcessMonitor(DementorSettings appsettings, ILogger<Worker> logger)
        {
            this.appsettings = appsettings;
            this.logger = logger;

            var retrievedRemoteBlacklist = true;
            try
            {
                logger.LogDebug("Retrieving blacklist...");
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
                logger.LogDebug("Blacklist retrieved.");
            }
            catch(Exception ex)
            {
                retrievedRemoteBlacklist = false;
            }

            //fall back to serialize from local file for now
            if (!retrievedRemoteBlacklist)
            {
                logger.LogDebug("Failed to retrieve blacklist. Using local list.");

                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), appsettings.BlacklistFile);

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        this.blackList = JsonConvert.DeserializeObject<BlackList>(reader.ReadToEnd());
                    }
                }
            }
        }

        public void Scan()
        {
            logger.LogDebug("Scanning processes...");
            var processes = Process.GetProcesses();

            //just do desktop apps for now
            foreach (var a in blackList.Apps)
            {
                foreach (var p in processes.Where(x => x.ProcessName.ToLower().Equals(a.Name.ToLower())))
                {
                    a.Detected = true;
                    a.Process = p;
                }
            }

            if (blackList.Apps.Where(x => x.Detected).Count() == 0)
            {
                logger.LogDebug("No blacklisted apps detected.");
                return;
            }

            logger.LogDebug("Found one more more blacklisted apps.");

            //collect the detected apps that warrant an email
            var x = blackList.Apps.Where(x => x.Detected && x.Action.HasFlag(BlackList.Action.NOTIFY));
            if (x.Count() > 0)
            {
                var naughtyApps = String.Join(',', x.Select(x => x.Name).ToArray());
                var user = GetProcessOwner(x.First().Process.Id);

                var ec = new Email();
                ec.From = appsettings.EmailFrom;
                ec.To = appsettings.EmailTo;
                ec.SmtpAddress = appsettings.EmailSmtpAddress;
                ec.SmtpPort = appsettings.EmailSmtpPort;
                ec.Subject = "Detected prohibited app usage";
                ec.Body = $"User {user} had the following apps running: {naughtyApps} on host: {Environment.MachineName}";
                ec.Username = appsettings.EmailUserName;
                ec.Password = appsettings.EmailPassword;

                ec.SendEmail();
            }

            //issue a pop-up to the naughty person
            x = blackList.Apps.Where(x => x.Detected && x.Action.HasFlag(BlackList.Action.MESSAGE));
            if (x.Count() > 0)
            {
                var naughtyApps = String.Join(',', x.Select(x => x.Name).ToArray());
                var owner = GetProcessOwner(x.First().Process.Id);
                var ps = new ProcessStartInfo("msg", $"{owner} \"You're not supposed to be using: {naughtyApps}");
                ps.UseShellExecute = true;
                Process.Start(ps);
            }

            //kill the prohibited processes
            x = blackList.Apps.Where(x => x.Detected && x.Action.HasFlag(BlackList.Action.MESSAGE));
            logger.LogDebug("Killing prohibited processeses.");
            foreach (var p in x)
            {
                if (!p.Process.CloseMainWindow())
                    p.Process.Kill(true);                    
            }
        }

        private string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            var searcher = new ManagementObjectSearcher(query);
            var processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {

                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[0];
                }
            }

            return "NO OWNER";
        }
    }
}

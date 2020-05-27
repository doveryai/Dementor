using System;
using System.ServiceProcess;

namespace Dementor
{
    class Program
    {
        static void Main(string[] args)
        {

            var pm = new ProcessMonitor();

            pm.Start();

            while (true)
            {
                System.Threading.Thread.Sleep(60000);
            }

            //ServiceBase.Run(new WindowsService());
        }
    }
}

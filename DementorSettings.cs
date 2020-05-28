using System;
using System.Collections.Generic;
using System.Text;

namespace Dementor
{
    public class DementorSettings
    {
        public int ProcessPollingInterval { get; set; }

        public string EmailFrom { get; set; }

        public string EmailTo { get; set; }

        public string EmailSmtpAddress { get; set; }

        public int EmailSmtpPort { get; set; }

        public string EmailUserName { get; set; }

        public string EmailPassword { get; set; }

        public string BlacklistFile { get; set; }
    }
}

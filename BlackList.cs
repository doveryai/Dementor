using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Dementor
{
    public class BlackList
    {
        [Flags]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum Action { KILL = 0x1, NOTIFY = 0x2, MESSAGE = 0x4}

        public List<Item> Apps { get; set; }

        public List<Item> BrowserTabs { get; set; }

        public BlackList()
        {
        }

        public class Item
        {
            public string Name { get; set; }

            public Action Action { get; set; }

            [JsonIgnore]
            public bool Detected { get; set; }

            [JsonIgnore]
            public Process Process { get; set; }
        }

        public class App : Item
        {            
        }

        public class BrowserTab: Item
        {
            public string domain { get; set; }
        }
    }
}

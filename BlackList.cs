using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dementor
{
    public class BlackList
    {
        [Flags]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum Action { KILL = 0x1, NOTIFY = 0x2 }

        public List<Item> Apps { get; set; }

        public List<Item> BrowserTabs { get; set; }

        public BlackList()
        {
            Apps = new List<Item>
            {
                new App
                {
                    Name = "Pooh",
                    Action = Action.NOTIFY
                },
                new App
                {
                    Name = "Pee",
                    Action = Action.KILL | Action.NOTIFY
                },
                new App
                {
                    Name = "Pop",
                    Action = Action.NOTIFY
                },
                new BrowserTab
                {
                    Name = "Roblox",
                    Action = Action.NOTIFY
                }
            };

            BrowserTabs = new List<Item>
            {
                new BrowserTab
                {
                    Name = "Roblox",
                    Action = Action.NOTIFY
                }
            };
        }

        public class Item
        {
            public string Name { get; set; }

            public Action Action { get; set; }
        }

        public class App : Item
        {

        }

        private class BrowserTab: Item
        {

        }
    }
}

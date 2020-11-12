using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Automation;
using Interop.UIAutomationClient;

namespace Dementor
{
    public class ChromeInspector
    {
        public void Inspect()
        {
            var procsChrome = Process.GetProcessesByName("chrome");
            if (procsChrome.Length <= 0)
            {
                Console.WriteLine("Chrome is not running");
            }
            else
            {
                foreach (var proc in procsChrome)
                {
                    // the chrome process must have a window 
                    if (proc.MainWindowHandle == IntPtr.Zero)
                    {
                        continue;
                    }
                }
            }
        }


        public void CUIAutomationSearch()
        {
            //var uiaClassObject = new CUIAutomation();
            //var chromeMainUIAElement = uiaClassObject.ElementFromHandle(proc.MainWindowHandle);

            ////UIA_ControlTypePropertyId =30003, UIA_TabItemControlTypeId = 50019
            //var chromeTabCondition = uiaClassObject.CreatePropertyCondition(30003, 50019);
            //var chromeTabCollection = chromeMainUIAElement.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, chromeTabCondition);

            //Console.WriteLine($"Found {chromeTabCollection.Length} tabs in chrome browser with pid: {proc.Id}.");

            //for (int i = 0; i < chromeTabCollection.Length; i++)
            //{
            //    var item = chromeTabCollection.GetElement(i);

            //    item.SetFocus();
            //    var hwnd = item.CurrentNativeWindowHandle;
            //    if (false)
            //        System.Windows.Forms.SendKeys.SendWait("^{w}");
            //    //Console.WriteLine($"Tab: {item.CurrentName}");
            //}

            ////UIA_NamePropertyId 30005
            //var chromeSearchbarCondition = uiaClassObject.CreatePropertyCondition(30005, "Address and search bar");
            //var chromeSearchbar = chromeMainUIAElement.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, chromeSearchbarCondition);

            //tagPOINT tp = new tagPOINT() { x = 0, y = 0 };
            //for (int i = 0; i < chromeSearchbar.Length; i++)
            //{
            //    var item = chromeSearchbar.GetElement(i);
            //    //https://docs.microsoft.com/en-us/windows/win32/winauto/uiauto-controlpattern-ids
            //    if (false)
            //        System.Windows.Forms.SendKeys.SendWait("^{w}");
            //}

            //can't find the right call to get value from Interop.Automation, use the old method
            //var root = AutomationElement.FromHandle(proc.MainWindowHandle);
            //var condSearch = new PropertyCondition(AutomationElement.NameProperty, "Address and search bar", System.Windows.Automation.PropertyConditionFlags.IgnoreCase);
            //var search = root.FindFirst(System.Windows.Automation.TreeScope.Descendants, condSearch);

            //var point = new System.Windows.Point();
            //point.X = tp.x;
            //point.Y = tp.y;

            //var search = AutomationElement.FromPoint(point);

            //if (search.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
            //{
            //    var valuePattern = (ValuePattern)patternObj;
            //    var url = valuePattern.Current.Value;

            //    Console.WriteLine($"Chrome is pointing to {url}.");
            //}
        }
    }
}

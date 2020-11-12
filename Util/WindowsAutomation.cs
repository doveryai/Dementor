using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;

namespace Dementor.Util
{
	//https://msdn.microsoft.com/en-us/library/windows/desktop/ee684076(v=vs.85).aspx
	//https://www.codeproject.com/Articles/141842/Automate-your-UI-using-Microsoft-Automation-Framew
	//http://testapi.codeplex.com/

	public static class WindowsAutomationMappingExtensions
    {
        public static string GetMapDescription(this WindowsAutomationMapping.Map map)
        {
            return String.Concat(
                String.IsNullOrEmpty(map.Name) ? "" : String.Format("Name: '{0}' ", map.Name),
                String.IsNullOrEmpty(map.ClassName) ? "" : String.Format("ClassName: '{0}' ", map.ClassName),
                String.IsNullOrEmpty(map.AutomationId) ? "" : String.Format("AutomationId: '{0}' ", map.AutomationId),
                String.IsNullOrEmpty(map.ControlTypeLocalizedControlType) ? "" : String.Format("ControlTypeLocalizedControlType: '{0}' ", map.ControlTypeLocalizedControlType)
                );
        }

        public static string GetElementDescription(this WindowsAutomationMapping.Map map)
        {
            var ae = map.Element as AutomationElement;
            if (ae == null)
                return String.Empty;

            var e = ae.Current;

            return String.Concat(
                String.IsNullOrEmpty(e.Name) ? "" : String.Format("Name: '{0}' ", e.Name),
                String.IsNullOrEmpty(e.ClassName) ? "" : String.Format("ClassName: '{0}' ", e.ClassName),
                String.IsNullOrEmpty(e.AutomationId) ? "" : String.Format("AutomationId: '{0}' ", e.AutomationId),
                String.IsNullOrEmpty(e.LocalizedControlType) ? "" : String.Format("ControlTypeLocalizedControlType: '{0}' ", e.LocalizedControlType)
                );
        }

		public static string Serialize(this AutomationElement element)
		{
			var el = element.Current;

			return $"ClassName: {el.ClassName}; Name: {el.Name}; AutomationId: {el.AutomationId}.";

			//var e = new XElement(String.IsNullOrEmpty(el.ClassName) ? "N/A" : el.ClassName);
			//e.Add(String.IsNullOrEmpty(el.AutomationId) ? "N/A" : el.AutomationId);
			//e.Add(String.IsNullOrEmpty(el.Name) ? "N/A" : el.Name);
			//return e.FromXElement<string>();
		}

		public static XElement ToXElement(this AutomationElement element)
		{
			var el = element.Current;

			var e = new XElement("Element");

			var cn = new XElement("ClassName");
			cn.Add(el.ClassName.ReplaceInvalidCharacters());

			var n = new XElement("Name");
			n.Add(el.Name.ReplaceInvalidCharacters());

			var aid = new XElement("AutomationId");
			aid.Add(el.AutomationId.ReplaceInvalidCharacters());


			e.Add(cn);
			e.Add(n);
			e.Add(aid);

			return e;
		}

		public static string ReplaceInvalidCharacters(this string input)
		{
			var otherInvalidCharacters = new List<char> { '+' };

			var result  = new string(input.ToCharArray().Select(x => Path.GetInvalidFileNameChars().Contains(x) || otherInvalidCharacters.Contains(x) ? '_' : x).ToArray());
			return result;
		}

		public static XElement ToXElement<T>(this object obj)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (TextWriter streamWriter = new StreamWriter(memoryStream))
				{
					var xmlSerializer = new XmlSerializer(typeof(T));
					xmlSerializer.Serialize(streamWriter, obj);
					return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
				}
			}
		}

		public static T FromXElement<T>(this XElement xElement)
		{
			var xmlSerializer = new XmlSerializer(typeof(T));
			return (T)xmlSerializer.Deserialize(xElement.CreateReader());
		}

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
		[DllImport("user32.dll")]
		public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

		public static void PrintWindow(IntPtr hwnd, string filePath)
		{
			if (hwnd == IntPtr.Zero)
				return;
				
			RECT rc;
			GetWindowRect(hwnd, out rc);

			Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
			Graphics gfxBmp = Graphics.FromImage(bmp);
			IntPtr hdcBitmap = gfxBmp.GetHdc();

			PrintWindow(hwnd, hdcBitmap, 0);

			gfxBmp.ReleaseHdc(hdcBitmap);
			gfxBmp.Dispose();

			bmp.Save(filePath);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			private int _Left;
			private int _Top;
			private int _Right;
			private int _Bottom;

			public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
			{
			}
			public RECT(int Left, int Top, int Right, int Bottom)
			{
				_Left = Left;
				_Top = Top;
				_Right = Right;
				_Bottom = Bottom;
			}

			public int X
			{
				get { return _Left; }
				set { _Left = value; }
			}
			public int Y
			{
				get { return _Top; }
				set { _Top = value; }
			}
			public int Left
			{
				get { return _Left; }
				set { _Left = value; }
			}
			public int Top
			{
				get { return _Top; }
				set { _Top = value; }
			}
			public int Right
			{
				get { return _Right; }
				set { _Right = value; }
			}
			public int Bottom
			{
				get { return _Bottom; }
				set { _Bottom = value; }
			}
			public int Height
			{
				get { return _Bottom - _Top; }
				set { _Bottom = value + _Top; }
			}
			public int Width
			{
				get { return _Right - _Left; }
				set { _Right = value + _Left; }
			}
			public Point Location
			{
				get { return new Point(Left, Top); }
				set
				{
					_Left = value.X;
					_Top = value.Y;
				}
			}
			public Size Size
			{
				get { return new Size(Width, Height); }
				set
				{
					_Right = value.Width + _Left;
					_Bottom = value.Height + _Top;
				}
			}

			public static implicit operator Rectangle(RECT Rectangle)
			{
				return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
			}
			public static implicit operator RECT(Rectangle Rectangle)
			{
				return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
			}
			public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
			{
				return Rectangle1.Equals(Rectangle2);
			}
			public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
			{
				return !Rectangle1.Equals(Rectangle2);
			}

			public override string ToString()
			{
				return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
			}

			public override int GetHashCode()
			{
				return ToString().GetHashCode();
			}

			public bool Equals(RECT Rectangle)
			{
				return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
			}

			public override bool Equals(object Object)
			{
				if (Object is RECT)
				{
					return Equals((RECT)Object);
				}
				else if (Object is Rectangle)
				{
					return Equals(new RECT((Rectangle)Object));
				}

				return false;
			}
		}
	}


	public class WindowsAutomation
    {
		public static decimal TimeDilation => decimal.Parse(ConfigurationManager.AppSettings["UIAutomationTimeDilation"]);
        private readonly ILogger<Worker> _logger;

        public WindowsAutomation(FileInfo fileInfo, ILogger<Worker> logger)
		{
			_mapping = Parse(fileInfo);
            _logger = logger;
			_logger.LogInformation($"UI time dilation value: {TimeDilation}");
		}

		private readonly WindowsAutomationMapping _mapping;

        public bool Apply(Func<string, string> environmentVariableLookup = null)
        {
            try
            {
                foreach (var map in _mapping.Maps)
                    map.Apply(environmentVariableLookup);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed, in part or in full, to apply Windows UI automation.", ex);
                return false;
            }
        }

        private static WindowsAutomationMapping Parse(FileInfo fileInfo, ILogger<Worker> logger)
        {
            var automationMappings = new WindowsAutomationMapping();

            if (!fileInfo.Exists)
            {
                logger.LogWarning(String.Format("Path to automation mappings could not be found: {0}", fileInfo.FullName));
            }

            try
            {
                var l = new Type[6];
                l[0] = typeof(WindowsAutomationMapping.Map);
                l[1] = typeof(WindowsAutomationMapping.Map.Input);
                l[2] = typeof(WindowsAutomationMapping.Map.Input.InputTypes);
                l[3] = typeof(WindowsAutomationMapping.Map.CustomCondition);
                l[4] = typeof(WindowsAutomationMapping.Map.CustomCondition.ConditionTypes);
                l[5] = typeof(WindowsAutomationMapping.Map.CustomCondition.Actions);

                var serializer = new XmlSerializer(typeof(WindowsAutomationMapping), l);

                using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                {
                    automationMappings = serializer.Deserialize(fs) as WindowsAutomationMapping;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(string.Format("Exception during deserialize automation mapping file: {0}", fileInfo.FullName), ex);
            }

            automationMappings.Logger = logger;

            return automationMappings;
        }
    }

    [DataContract(Name = "AutomationMappings", Namespace = "")]
    public class WindowsAutomationMapping
    {
        public List<Map> Maps = new List<Map>();

        public ILogger<Worker> Logger { get; set; }

        public class Map
        {
			private TreeScope? scope;

            public TreeScope Scope
            {
                //automationIDs are often undefined, but they allow efficient Descendant search, so default to that if it's not explicit
                get { return scope.HasValue ? scope.Value : (String.IsNullOrEmpty(AutomationId) ? TreeScope.Children : TreeScope.Descendants); }
                set { scope = value; }
            }

            // Element and ParentMap are not serialized because they are just helper properties when we navigate the tree
            public object Element { get; set; }

            public IntPtr NativeWindowsHandle
            {
                get
                {
                    if (Element == null)
                        return IntPtr.Zero;

                    var element = Element as AutomationElement;
                    if (element.Current.NativeWindowHandle != 0)
                        return (IntPtr)element.Current.NativeWindowHandle;
                    
                    var parent = ParentMap;

                    while (parent != null)
                    {
                        if (parent.Element == null)
                            continue;

                        var parentElement = parent.Element as AutomationElement;
                        if (parentElement.Current.NativeWindowHandle != 0)
                            return (IntPtr)parentElement.Current.NativeWindowHandle;

                        parent = parent.ParentMap;
                    }

                    return IntPtr.Zero;
                }
            }

            public Map ParentMap { get; set; }

            public bool HasControlTypeIdentifiers()
            {
                return (
                    ControlTypeId > 0 ||
                    !String.IsNullOrEmpty(ControlTypeLocalizedControlType) ||
                    !String.IsNullOrEmpty(ControlTypeProgrammaticName)
                    );
            }

            public int ControlTypeId { get; set; }

            public string ControlTypeLocalizedControlType { get; set; }

            public string ControlTypeProgrammaticName { get; set; }

            public string ClassName { get; set; }

            public string Name { get; set; }

            public string AutomationId { get; set; }

            public string Tag { get; set; }

            public List<Input> Inputs = new List<Input>();

            public List<Map> Maps = new List<Map>();

            public List<CustomCondition> CustomConditions = new List<CustomCondition>();

            private void BindElement(Func<string, string> environmentVariableLookup = null)
            {
                //first, build the condition
                Condition condition = null;

                if (!String.IsNullOrEmpty(Tag))
                {
                    _logger.(TraceEventType.Verbose, this.Tag);
                }

                var conditions = new List<PropertyCondition>();

                if (!String.IsNullOrEmpty(ClassName))
                    conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, ClassName));
                if (!String.IsNullOrEmpty(Name))
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, EnvironmentVariableSubstitution(Name, environmentVariableLookup)));
                if (!String.IsNullOrEmpty(AutomationId))
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, AutomationId));
                if (!String.IsNullOrEmpty(ControlTypeLocalizedControlType))
                    conditions.Add(new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, ControlTypeLocalizedControlType));

                //if we have more than one condition
                if (conditions.Count == 0)
                {
                    throw new ArgumentNullException("ClassName, Name, ControlType and AutomationId cannot all be null.");
                }
                else if (conditions.Count > 1)
                {
                    condition = new AndCondition(conditions.ToArray());
                }
                //otherwise just create the condition direcly
                else
                    condition = conditions[0];

                //.NET doesn't support TreeScope.Ancestors natively, so we'll implement by searching back up the chain
                AutomationElement element = null;
                if (this.Scope == TreeScope.Ancestors)
                {
                    element = FindFirstInAncestors(this, condition);
                }
                else
                {
                    var parentElement = ParentMap == null ? AutomationElement.RootElement : ParentMap.Element as AutomationElement;
                    element = parentElement.FindFirst(this.Scope, condition);
                }

                if (element != null && CustomConditions.Count > 0)
                {
                    //just assume AND operation for all custom conditions for now
                    foreach (var cond in CustomConditions)
                    {
                        if (cond.Type == CustomCondition.ConditionTypes.MapExists)
                        {
                            cond.Map.ParentMap = ParentMap;
                            cond.Map.BindElement(environmentVariableLookup);
                            if (cond.Map.Element == null)
                            {
                                BreezyTrace.Log(TraceEventType.Information, "Custom 'MapExists' condition defined but was not satisfied. No additional action taken.");
                                break;
                            }
                            else
                            {
                                BreezyTrace.Log(TraceEventType.Information, "Custom 'MapExists' condition returned 'true'.");

                                switch (cond.Action)
                                {
                                    case CustomCondition.Actions.Skipover:
                                        BreezyTrace.Log(TraceEventType.Information, "Skipping current element.");
                                        element = null;
                                        break;
                                    case CustomCondition.Actions.Continue:
                                        BreezyTrace.Log(TraceEventType.Information, "Continuing to process current element.");
                                        break;
                                    case CustomCondition.Actions.Abort:
                                        BreezyTrace.Log(TraceEventType.Information, "Aborting current element (aborting the processing chain).");
                                        element = null;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                this.Element = element;
            }

            private string EnvironmentVariableSubstitution(string value, Func<string, string> environmentVariableLookup)
            {
                if (environmentVariableLookup == null || !value.StartsWith("$"))
                    return value;

                //we should look this up but first...
                //strip the $ and any non-alphanums
                var regex = new Regex(@"\${1}(\w*|\d*)");
                if (!regex.IsMatch(value))
                    return value;

                var property = regex.Match(value).Groups[1].Value;

                var newValue = value;
                try
                {
                    newValue = environmentVariableLookup(property);
                }
                catch (Exception ex)
                {
                    BreezyTrace.Log(TraceEventType.Error, String.Format("Environment variable lookup failed for {0}", value), ex);
                    return value;
                }

                //just return the value if we don't get anything
                if (String.IsNullOrEmpty(newValue))
                    return value;

                //replace whatever may have come after something like "\subfolder\stuff"
                value = value.Replace("$" + property, newValue);

                return value;
            }

            public void Apply(Func<string, string> environmentVariableLookup = null, WindowsAutomationMapping.Map parentMap = null)
            {
                ParentMap = parentMap;

                var mapDesc = WindowsAutomationMappingExtensions.GetMapDescription(this);
                BreezyTrace.Log(TraceEventType.Verbose, String.Format("Searching for element for map {0}...", mapDesc));

                BindElement(environmentVariableLookup);

                if (Element == null)
                {
                    BreezyTrace.Log(TraceEventType.Verbose, String.Format("Element for map {0} could not be found or its conditions were not met. Terminating this search chain.", mapDesc));
                    return;
                }

                BreezyTrace.Log(TraceEventType.Verbose, String.Format("Found element. {0}.", WindowsAutomationMappingExtensions.GetElementDescription(this)));

                var element = Element as AutomationElement;

				ApplyInputs(environmentVariableLookup);

                //recurse through any child maps
                foreach (var childMap in Maps)
                {
                    childMap.Apply(environmentVariableLookup, this);
                }
            }

            private void ApplyInputs(Func<string, string> environmentVariableLookup = null)
            {
                foreach (var input in Inputs)
                {
                    var element = Element as AutomationElement;
                    object pattern = null;

                    try
                    {
                        switch (input.Type)
                        {
                            case WindowsAutomationMapping.Map.Input.InputTypes.Direction:
                                var hwnd = NativeWindowsHandle;
                                BreezyTrace.Log(TraceEventType.Verbose, String.Format("Sending direction key: {0} to hwnd: {1}", input.Value, hwnd));
                                var item = new EnumWindowsItem(hwnd, WindowsAutomation.TimeDilation);
                                switch (input.Value)
                                {
                                    case "Up":
                                        item.SendUp();
                                        break;
                                    case "Down":
                                        item.SendDown();
                                        break;
                                    case "Left":
                                        item.SendLeft();
                                        break;
                                    case "Right":
                                        item.SendRight();
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Esc:
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Invoke:
                                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                                    (pattern as InvokePattern).Invoke();
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.ExpandCollapse:
                                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out pattern))
                                {
                                    element.SetFocus();
                                    var expandCollapse = pattern as ExpandCollapsePattern;
                                    if (
                                        (expandCollapse.Current.ExpandCollapseState.HasFlag(ExpandCollapseState.Collapsed) || expandCollapse.Current.ExpandCollapseState.HasFlag(ExpandCollapseState.PartiallyExpanded)) &&
                                        input.Value == "Expand"
                                        )
                                    {
                                        expandCollapse.Expand();
                                    }
                                    else if (input.Value == "Collapse")
                                        expandCollapse.Collapse();
                                }
                                break;
							case WindowsAutomationMapping.Map.Input.InputTypes.SendKey:
								hwnd = NativeWindowsHandle;
								item = new EnumWindowsItem(hwnd, WindowsAutomation.TimeDilation);
								var b = Convert.ToByte(input.Value);
								item.SendKey(b);
								break;
							case WindowsAutomationMapping.Map.Input.InputTypes.Letter:
								//hwnd = NativeWindowsHandle;
								//item = new EnumWindowsItem(hwnd, WindowsAutomation.TimeDilation);
								//item.SendKey(input.Value);
								break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Number:
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Return:
                                hwnd = NativeWindowsHandle;
                                BreezyTrace.Log(TraceEventType.Verbose, String.Format("Sending Enter key: {0} to hwnd: {1}", input.Value, hwnd));
                                item = new EnumWindowsItem(hwnd, WindowsAutomation.TimeDilation);
                                item.SendReturn();
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.SelectionItem:
                                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                                    (pattern as SelectionItemPattern).Select();
                                break;
							case WindowsAutomationMapping.Map.Input.InputTypes.Selection:
								//doesn't do anything
								break;
							case WindowsAutomationMapping.Map.Input.InputTypes.Tab:
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Text:
                                //check to see if we're referencing an WF services environment variable (e.g. TempDir) and if so, get that value
                                var val = EnvironmentVariableSubstitution(input.Value, environmentVariableLookup);

                                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out pattern))
                                    (pattern as ValuePattern).SetValue(val);
                                break;

                            case WindowsAutomationMapping.Map.Input.InputTypes.Toggle:
                                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                                {
                                    var toggle = pattern as TogglePattern;
									if (
										(toggle.Current.ToggleState == ToggleState.Off && input.Value == "On") ||
										(toggle.Current.ToggleState == ToggleState.On && input.Value == "Off")
										)
									{
										toggle.Toggle();
									}
                                }
                                break;
                            case WindowsAutomationMapping.Map.Input.InputTypes.Wait:
								var delay = Decimal.ToInt32(Convert.ToInt32(input.Value) * WindowsAutomation.TimeDilation);
                                Thread.Sleep(delay);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        BreezyTrace.Log(TraceEventType.Information, "Input failed for this mapping.", ex);
                    }

                    BreezyTrace.Log(TraceEventType.Verbose, String.Format("Successfully applied input type: {0}", input.Type));
                }
            }

            private static AutomationElement FindFirstInAncestors(Map map, Condition condition)
            {
                if (map.ParentMap == null || map.ParentMap.Element == null)
                {
                    BreezyTrace.Log(TraceEventType.Verbose, String.Format("Terminated automation search in ancestor elements at the root (or the map is missing a parent reference)."));
                    return null;
                }

                var parentMap = map.ParentMap;
                var parentElement = parentMap.Element as AutomationElement;

                var e = parentElement.FindFirst(TreeScope.Element, condition);

                //keep going up until we find the element or terminate at the root
                if (e == null)
                {
                    e = FindFirstInAncestors(parentMap, condition);
                }

                return e;
            }

            public class Input
            {
                public enum InputTypes { Direction, Esc, ExpandCollapse, Invoke, SendKey, Letter, Number, Return, SelectionItem, Selection, Tab, Text, Toggle, Wait };

                public string Value { get; set; }

                public InputTypes Type { get; set; }
            }

            public class CustomCondition
            {
                public enum ConditionTypes { MapExists };

                public enum Actions { Continue, Skipover, Abort };

                public ConditionTypes Type { get; set; }

                public Actions Action { get; set; }

                public Map Map { get; set; }
            }
        }
    }
}

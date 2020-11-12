using System;
using System.Collections;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Dementor.Util
{
	#region enums

	/// <summary>
	/// Window Style Flags
	/// </summary>
	[Flags]
	public enum WindowStyleFlags : uint
	{
		WS_OVERLAPPED = 0x00000000,
		WS_POPUP = 0x80000000,
		WS_CHILD = 0x40000000,
		WS_MINIMIZE = 0x20000000,
		WS_VISIBLE = 0x10000000,
		WS_DISABLED = 0x08000000,
		WS_CLIPSIBLINGS = 0x04000000,
		WS_CLIPCHILDREN = 0x02000000,
		WS_MAXIMIZE = 0x01000000,
		WS_BORDER = 0x00800000,
		WS_DLGFRAME = 0x00400000,
		WS_VSCROLL = 0x00200000,
		WS_HSCROLL = 0x00100000,
		WS_SYSMENU = 0x00080000,
		WS_THICKFRAME = 0x00040000,
		WS_GROUP = 0x00020000,
		WS_TABSTOP = 0x00010000,
		WS_MINIMIZEBOX = 0x00020000,
		WS_MAXIMIZEBOX = 0x00010000,
	}

	/// <summary>
	/// Extended Windows Style flags
	/// </summary>
	[Flags]
	public enum ExtendedWindowStyleFlags
	{
		WS_EX_DLGMODALFRAME = 0x00000001,
		WS_EX_NOPARENTNOTIFY = 0x00000004,
		WS_EX_TOPMOST = 0x00000008,
		WS_EX_ACCEPTFILES = 0x00000010,
		WS_EX_TRANSPARENT = 0x00000020,

		WS_EX_MDICHILD = 0x00000040,
		WS_EX_TOOLWINDOW = 0x00000080,
		WS_EX_WINDOWEDGE = 0x00000100,
		WS_EX_CLIENTEDGE = 0x00000200,
		WS_EX_CONTEXTHELP = 0x00000400,

		WS_EX_RIGHT = 0x00001000,
		WS_EX_LEFT = 0x00000000,
		WS_EX_RTLREADING = 0x00002000,
		WS_EX_LTRREADING = 0x00000000,
		WS_EX_LEFTSCROLLBAR = 0x00004000,
		WS_EX_RIGHTSCROLLBAR = 0x00000000,

		WS_EX_CONTROLPARENT = 0x00010000,
		WS_EX_STATICEDGE = 0x00020000,
		WS_EX_APPWINDOW = 0x00040000,

		WS_EX_LAYERED = 0x00080000,

		WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
		WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring

		WS_EX_COMPOSITED = 0x02000000,
		WS_EX_NOACTIVATE = 0x08000000
	}
	#endregion

	#region EnumWindows
	/// <summary>
	/// EnumWindows wrapper for .NET
	/// </summary>
	public class EnumWindows
	{
		#region Delegates
		public delegate int WindowEnumeratedEvent(IntPtr hwnd, int lParam);
		public event WindowEnumeratedEvent WindowEnumerated;
		#endregion

		#region UnManagedMethods

		private class UnManagedMethods
		{
			[DllImport("user32")]
			public static extern int EnumWindows(WindowEnumeratedEvent lpEnumFunc, int lParam);

			[DllImport("user32")]
			public static extern int EnumChildWindows(IntPtr hWndParent, WindowEnumeratedEvent lpEnumFunc, int lParam);

			[DllImport("user32")]
			public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

			[DllImport("user32")]
			public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName,
													  string windowTitle);

			[DllImport("user32")]
			public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

			[DllImport("kernel32", SetLastError = true)]
			public static extern void SetLastError(int dwErrCode);
		}

		#endregion

		#region Member Variables
		private EnumWindowsCollection items = null;
		#endregion

		/// <summary>
		/// Returns the collection of windows returned by
		/// GetWindows
		/// </summary>
		public EnumWindowsCollection Items
		{
			get { return this.items; }
		}

		/// <summary>
		/// Gets all top level windows on the system.
		/// </summary>
		public int GetWindows()
		{
			int success = UnManagedMethods.EnumWindows(new WindowEnumeratedEvent(this.WindowEnum), 0);
			return success == 1 ? 0 : Marshal.GetLastWin32Error();
		}

		/// <summary>
		/// Gets all child windows of the specified window
		/// </summary>
		/// <param name="hWndParent">Window Handle to get children for</param>
		public int GetWindows(IntPtr hWndParent)
		{
			this.items = new EnumWindowsCollection();
			int success = UnManagedMethods.EnumChildWindows(hWndParent, new WindowEnumeratedEvent(this.WindowEnum), 0);
			return success == 1 ? 0 : Marshal.GetLastWin32Error();
		}

		public static EnumWindowsItem FindWindows(string caption, string className)
		{
			IntPtr hWindow = UnManagedMethods.FindWindow(caption, className);
			return hWindow != IntPtr.Zero ? new EnumWindowsItem(hWindow) : null;
		}

		public static IntPtr FindWindowsEx(string caption, string className, IntPtr hWndParent)
		{
			var _caption = String.IsNullOrEmpty(caption) ? null : caption;
			var _className = String.IsNullOrEmpty(className) ? null : className;

			IntPtr hWindow = UnManagedMethods.FindWindowEx(hWndParent, IntPtr.Zero, _className, _caption);

			if (hWindow == IntPtr.Zero)
				hWindow = UnManagedMethods.FindWindow(_className, _caption);

			return hWindow;
		}

		#region EnumWindows callback

		/// <summary>
		/// The enum Windows callback.
		/// </summary>
		/// <param name="hWnd">Window Handle</param>
		/// <param name="lParam">Application defined value</param>
		/// <returns>1 to continue enumeration, 0 to stop</returns>
		public int WindowEnum(IntPtr hWnd, int lParam)
		{
			int result = 0;
			//using (var e = new EnumWindowsItem(hWnd)) {
			//    string t = e.Text;
			//    string clss = e.ClassName;
			//    Breezy.Common.Logging.BreezyTrace.Log(System.Diagnostics.TraceEventType.Information, string.Format("window count: {0}, class: {1}, text: {2}", count, t, clss));
			//}

			if (WindowEnumerated != null)
			{
				result = WindowEnumerated(hWnd, lParam);
			}

			UnManagedMethods.SetLastError(0);
			return result;
		}

		#endregion

		#region Constructor, Dispose

		public EnumWindows()
		{
		}

		#endregion
	}
	#endregion EnumWindows

	#region EnumWindowsCollection
	/// <summary>
	/// Holds a collection of Windows returned by GetWindows.
	/// </summary>
	public class EnumWindowsCollection : ReadOnlyCollectionBase
	{
		/// <summary>
		/// Add a new Window to the collection.  Intended for
		/// internal use by EnumWindows only.
		/// </summary>
		/// <param name="hWnd">Window handle to add</param>
		public void Add(IntPtr hWnd)
		{
			EnumWindowsItem item = new EnumWindowsItem(hWnd);
			this.InnerList.Add(item);
		}

		/// <summary>
		/// Gets the Window at the specified index
		/// </summary>
		public EnumWindowsItem this[int index]
		{
			get { return (EnumWindowsItem)this.InnerList[index]; }
		}

		/// <summary>
		/// Constructs a new EnumWindowsCollection object.
		/// </summary>
		public EnumWindowsCollection()
		{
		}
	}
	#endregion

	#region EnumWindowsItem
	/// <summary>
	/// Provides details about a Window returned by the 
	/// enumeration
	/// </summary>
	public class EnumWindowsItem : IDisposable
	{
		private readonly decimal timeDilation = 0;

		/// <summary>
		///  Constructs a new instance of this class for
		///  the specified Window Handle.
		/// </summary>
		/// <param name="hWnd">The Window Handle</param>
		public EnumWindowsItem(IntPtr hWnd, decimal timeDilation = 0)
		{
			this.timeDilation = timeDilation;
			this.hWnd = hWnd;
		}

		public EnumWindowsItem()
		{
		}

		public int Delay => Decimal.ToInt32(Math.Round(100 * timeDilation));

		private IntPtr hWnd;
		#region Structures

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct FLASHWINFO
		{
			public int cbSize;
			public IntPtr hwnd;
			public int dwFlags;
			public int uCount;
			public int dwTimeout;
		}

		#endregion

		#region UnManagedMethods

		public class UnManagedMethods
		{
			[DllImport("user32")]
			public static extern int IsWindowVisible(
				IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Auto)]
			public static extern int GetWindowText(
				IntPtr hWnd,
				StringBuilder lpString,
				int cch);

			[DllImport("user32", CharSet = CharSet.Auto)]
			public static extern int GetWindowTextLength(
				IntPtr hWnd);

			[DllImport("user32")]
			public static extern int BringWindowToTop(IntPtr hWnd);

			[DllImport("user32")]
			public static extern int SetForegroundWindow(IntPtr hWnd);

			[DllImport("user32")]
			public static extern int IsIconic(IntPtr hWnd);

			[DllImport("user32")]
			public static extern int IsZoomed(IntPtr hwnd);

			[DllImport("user32")]
			public static extern int SetFocus(IntPtr hwnd);

			[DllImport("user32", CharSet = CharSet.Auto)]
			public static extern int GetClassName(
				IntPtr hWnd,
				StringBuilder lpClassName,
				int nMaxCount);

			[DllImport("user32")]
			public static extern int FlashWindow(
				IntPtr hWnd,
				ref FLASHWINFO pwfi);

			[DllImport("user32")]
			public static extern int GetWindowRect(
				IntPtr hWnd,
				ref RECT lpRect);

			[DllImport("user32", CharSet = CharSet.Auto)]
			public static extern int SendMessage(
				IntPtr hWnd,
				int wMsg,
				IntPtr wParam,
				IntPtr lParam);

			[DllImport("user32", CharSet = CharSet.Auto)]
			public static extern uint GetWindowLong(
				IntPtr hwnd,
				int nIndex);

			[DllImport("user32.dll")]
			public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

			[DllImport("user32.dll")]
			public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll")]
			static extern int MapVirtualKey(uint uCode, uint uMapType);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			public static extern byte VkKeyScan(char ch);

			//Full list of scan codes: https://msdn.microsoft.com/en-us/library/aa299374(v=vs.60).aspx
			public const byte VK_NUMPAD7 = 0x67;
			public const byte VK_NUMPAD8 = 0x68;
			public const byte VK_NUMPAD9 = 0x69;
			public const byte VK_MULTIPLY = 0x6A;
			public const byte VK_ADD = 0x6B;
			public const byte VK_SEPARATOR = 0x6C;
			public const byte VK_SUBTRACT = 0x6D;
			public const byte VK_DECIMAL = 0x6E;
			public const byte VK_DIVIDE = 0x6F;
			public const byte VK_F1 = 0x70;
			public const byte VK_F2 = 0x71;
			public const byte VK_F3 = 0x72;
			public const byte VK_F4 = 0x73;
			public const byte VK_F5 = 0x74;
			public const byte VK_F6 = 0x75;
			public const byte VK_F7 = 0x76;
			public const byte VK_F8 = 0x77;
			public const byte VK_F9 = 0x78;
			public const byte VK_F10 = 0x79;
			public const byte VK_F11 = 0x7A;
			public const byte VK_F12 = 0x7B;
			public const byte VK_NUMLOCK = 0x90;
			public const byte VK_SCROLL = 0x91;
			public const byte VK_LSHIFT = 0xA0;
			public const byte VK_RSHIFT = 0xA1;
			public const byte VK_LCONTROL = 0xA2;
			public const byte VK_RCONTROL = 0xA3;
			public const byte VK_LMENU = 0xA4;
			public const byte VK_RMENU = 0xA5;
			public const byte VK_BACK = 0x08;
			public const byte VK_TAB = 0x09;
			public const byte VK_RETURN = 0x0D;
			public const byte VK_SHIFT = 0x10;
			public const byte VK_CONTROL = 0x11;
			public const byte VK_MENU = 0x12;
			public const byte VK_PAUSE = 0x13;
			public const byte VK_CAPITAL = 0x14;
			public const byte VK_ESCAPE = 0x1B;
			public const byte VK_SPACE = 0x20;
			public const byte VK_END = 0x23;
			public const byte VK_HOME = 0x24;
			public const byte VK_LEFT = 0x25;
			public const byte VK_UP = 0x26;
			public const byte VK_RIGHT = 0x27;
			public const byte VK_DOWN = 0x28;
			public const byte VK_PRINT = 0x2A;
			public const byte VK_SNAPSHOT = 0x2C;
			public const byte VK_INSERT = 0x2D;
			public const byte VK_DELETE = 0x2E;
			public const byte VK_LWIN = 0x5B;
			public const byte VK_RWIN = 0x5C;
			public const byte VK_NUMPAD0 = 0x60;
			public const byte VK_NUMPAD1 = 0x61;
			public const byte VK_NUMPAD2 = 0x62;
			public const byte VK_NUMPAD3 = 0x63;
			public const byte VK_NUMPAD4 = 0x64;
			public const byte VK_NUMPAD5 = 0x65;
			public const byte VK_NUMPAD6 = 0x66;

			public const byte VK_0 = 0x30;  //0 key
			public const byte VK_1 = 0x31;  //1 key
			public const byte VK_2 = 0x32;  //2 key
			public const byte VK_3 = 0x33;  //3 key
			public const byte VK_4 = 0x34;  //4 key
			public const byte VK_5 = 0x35;  //5 key
			public const byte VK_6 = 0x36;  //6 key
			public const byte VK_7 = 0x37;  //7 key
			public const byte VK_8 = 0x38;  //8 key
			public const byte VK_9 = 0x39;  //9 key
			public const byte VK_A = 0x41;  //A key
			public const byte VK_B = 0x42;  //B key
			public const byte VK_C = 0x43;  //C key
			public const byte VK_D = 0x44;  //D key
			public const byte VK_E = 0x45;  //E key
			public const byte VK_F = 0x46;  //F key
			public const byte VK_G = 0x47;  //G key
			public const byte VK_H = 0x48;  //H key
			public const byte VK_I = 0x49;  //I key
			public const byte VK_J = 0x4A;  //J key
			public const byte VK_K = 0x4B;  //K key
			public const byte VK_L = 0x4C;  //L key
			public const byte VK_M = 0x4D;  //M key
			public const byte VK_N = 0x4E;  //N key
			public const byte VK_O = 0x4F;  //O key
			public const byte VK_P = 0x50;  //P key
			public const byte VK_Q = 0x51;  //Q key
			public const byte VK_R = 0x52;  //R key
			public const byte VK_S = 0x53;  //S key
			public const byte VK_T = 0x54;  //T key
			public const byte VK_U = 0x55;  //U key
			public const byte VK_V = 0x56;  //V key
			public const byte VK_W = 0x57;  //W key
			public const byte VK_X = 0x58;  //X key
			public const byte VK_Y = 0x59;  //Y key
			public const byte VK_Z = 0x5A;  //Z key

			public const int WM_KEYDOWN = 0x0100;
			public const int WM_KEYUP = 0x0101;
			public const int WM_SYSKEYUP = 0x0105;

			public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
			public const int KEYEVENTF_KEYUP = 0x0002;

			public const int WM_COMMAND = 0x111;
			public const int WM_SYSCOMMAND = 0x112;

			public const int SC_RESTORE = 0xF120;
			public const int SC_CLOSE = 0xF060;
			public const int SC_MAXIMIZE = 0xF030;
			public const int SC_MINIMIZE = 0xF020;

			public const int GWL_STYLE = (-16);
			public const int GWL_EXSTYLE = (-20);

			/// <summary>
			/// Stop flashing. The system restores the window to its original state.
			/// </summary>
			public const int FLASHW_STOP = 0;

			/// <summary>
			/// Flash the window caption. 
			/// </summary>
			public const int FLASHW_CAPTION = 0x00000001;

			/// <summary>
			/// Flash the taskbar button.
			/// </summary>
			public const int FLASHW_TRAY = 0x00000002;

			/// <summary>
			/// Flash both the window caption and taskbar button.
			/// </summary>
			public const int FLASHW_ALL = (FLASHW_CAPTION | FLASHW_TRAY);

			/// <summary>
			/// Flash continuously, until the FLASHW_STOP flag is set.
			/// </summary>
			public const int FLASHW_TIMER = 0x00000004;

			/// <summary>
			/// Flash continuously until the window comes to the foreground. 
			/// </summary>
			public const int FLASHW_TIMERNOFG = 0x0000000C;
		}

		#endregion

		/// <summary>
		/// The window handle.
		/// </summary>

		/// <summary>
		/// To allow items to be compared, the hash code
		/// is set to the Window handle, so two EnumWindowsItem
		/// objects for the same Window will be equal.
		/// </summary>
		/// <returns>The Window Handle for this window</returns>
		public override System.Int32 GetHashCode()
		{
			return (System.Int32)this.hWnd;
		}

		/// <summary>
		/// Gets the window's handle
		/// </summary>
		public IntPtr Handle
		{
			get { return this.hWnd; }
		}

		/// <summary>
		/// Gets the window's title (caption)
		/// </summary>
		public string Text
		{
			get
			{
				StringBuilder title = new StringBuilder(400, 400);
				UnManagedMethods.GetWindowText(this.hWnd, title, title.Capacity);
				return title.ToString();
			}
		}

		/// <summary>
		/// Gets the window's class name.
		/// </summary>
		public string ClassName
		{
			get
			{
				StringBuilder className = new StringBuilder(260, 260);
				UnManagedMethods.GetClassName(this.hWnd, className, className.Capacity);
				return className.ToString();
			}
		}

		/// <summary>
		/// Gets/Sets whether the window is iconic (mimimised) or not.
		/// </summary>
		public bool Iconic
		{
			get { return UnManagedMethods.IsIconic(this.hWnd) != 0; }
			set
			{
				UnManagedMethods.SendMessage(
					this.hWnd,
					UnManagedMethods.WM_SYSCOMMAND,
					(IntPtr)UnManagedMethods.SC_MINIMIZE,
					IntPtr.Zero);
			}
		}

		/// <summary>
		/// Gets/Sets whether the window is maximised or not.
		/// </summary>
		public bool Maximised
		{
			get { return UnManagedMethods.IsZoomed(this.hWnd) != 0; }
			set
			{
				UnManagedMethods.SendMessage(
					this.hWnd,
					UnManagedMethods.WM_SYSCOMMAND,
					(IntPtr)UnManagedMethods.SC_MAXIMIZE,
					IntPtr.Zero);
			}
		}

		/// <summary>
		/// Gets whether the window is visible.
		/// </summary>
		public bool Visible
		{
			get { return UnManagedMethods.IsWindowVisible(this.hWnd) != 0; }
		}

		/// <summary>
		/// Gets the bounding rectangle of the window
		/// </summary>
		public System.Drawing.Rectangle Rect
		{
			get
			{
				RECT rc = new RECT();
				UnManagedMethods.GetWindowRect(
					this.hWnd,
					ref rc);
				System.Drawing.Rectangle rcRet = new System.Drawing.Rectangle(
					rc.Left, rc.Top,
					rc.Right - rc.Left, rc.Bottom - rc.Top);
				return rcRet;
			}
		}

		/// <summary>
		/// Gets the location of the window relative to the screen.
		/// </summary>
		public System.Drawing.Point Location
		{
			get
			{
				System.Drawing.Rectangle rc = Rect;
				System.Drawing.Point pt = new System.Drawing.Point(
					rc.Left,
					rc.Top);
				return pt;
			}
		}

		/// <summary>
		/// Gets the size of the window.
		/// </summary>
		public System.Drawing.Size Size
		{
			get
			{
				System.Drawing.Rectangle rc = Rect;
				System.Drawing.Size sz = new System.Drawing.Size(
					rc.Right - rc.Left,
					rc.Bottom - rc.Top);
				return sz;
			}
		}

		/// <summary>
		/// Restores and Brings the window to the front, 
		/// assuming it is a visible application window.
		/// </summary>
		public void Restore()
		{
			if (Iconic)
			{
				UnManagedMethods.SendMessage(
					this.hWnd,
					UnManagedMethods.WM_SYSCOMMAND,
					(IntPtr)UnManagedMethods.SC_RESTORE,
					IntPtr.Zero);
				Thread.Sleep(Delay);
			}
			UnManagedMethods.BringWindowToTop(this.hWnd);
			Thread.Sleep(Delay);
			UnManagedMethods.SetForegroundWindow(this.hWnd);
			Thread.Sleep(Delay);
		}

		public void SendKey(byte key, byte val)
		{
			//press down de key
			UnManagedMethods.keybd_event(key, val, UnManagedMethods.KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
			Thread.Sleep(Delay);
			//release de key
			UnManagedMethods.keybd_event(key, val,
										 UnManagedMethods.KEYEVENTF_KEYUP | UnManagedMethods.KEYEVENTF_EXTENDEDKEY,
										 UIntPtr.Zero);
			Thread.Sleep(Delay);
		}

		public void SendKey(string code, bool wait = false)
		{
			if (wait)
				System.Windows.Forms.SendKeys.SendWait(code);
			else
				System.Windows.Forms.SendKeys.Send(code);
			Thread.Sleep(Delay);
		}

		public void SendKey(byte key)
		{
			SendKey(key, key);
		}

		public void Focus()
		{
			var r = UnManagedMethods.SetFocus(this.hWnd);
			Thread.Sleep(Delay);
		}

		public void SendReturn()
		{
			SendKey(UnManagedMethods.VK_RETURN, 0x9C);
		}

		public void SendSpace()
		{
			SendKey(UnManagedMethods.VK_SPACE, UnManagedMethods.VK_SPACE);
		}

		public void SendCtrlShiftP(bool wait = false)
		{
			UnManagedMethods.keybd_event(UnManagedMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);     //control Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_SHIFT, 0, 0, UIntPtr.Zero);                 //shift Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, 0, UIntPtr.Zero);                 //P Key Pressed
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //alt Key released
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_SHIFT, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //n Key released
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_CONTROL, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //n Key released
			Thread.Sleep(Delay);
		}

		public void SendAltFPP()
		{
			//SendKey(UnManagedMethods.VK_ESCAPE, 0x1B);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_MENU, 0, 0, UIntPtr.Zero);     //alt Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_MENU, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //alt Key released
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_F, 0, 0, UIntPtr.Zero);                 //n Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_F, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //n Key released
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, 0, UIntPtr.Zero);                 //n Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //n Key released
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, 0, UIntPtr.Zero);                 //n Key Pressed
			Thread.Sleep(Delay);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_P, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //n Key released
			Thread.Sleep(Delay);
		}

		public void SendAltF4()
		{
			//SendKey(UnManagedMethods.VK_ESCAPE, 0x1B);
			UnManagedMethods.keybd_event(UnManagedMethods.VK_MENU, 0, 0, UIntPtr.Zero);     //alt Key Pressed
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_F4, 0, 0, UIntPtr.Zero);                 //F4 Key Pressed
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_MENU, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //alt Key released
			Thread.Sleep(Delay);

			UnManagedMethods.keybd_event(UnManagedMethods.VK_F4, 0, UnManagedMethods.KEYEVENTF_KEYUP, UIntPtr.Zero); //F4 Key released
			Thread.Sleep(Delay);
		}

		public void SendEscape()
		{
			SendKey(UnManagedMethods.VK_ESCAPE, 0x1B);
		}

		public void SendTab()
		{
			SendKey(UnManagedMethods.VK_TAB, 0x8F);
		}

		public void SendY()
		{
			SendKey(UnManagedMethods.VkKeyScan('Y'), 0x95); // ‘A’ Press
		}

		public void SendN()
		{
			SendKey(UnManagedMethods.VkKeyScan('N'), 0xB1); // ‘A’ Press
		}

		public void SendDown()
		{
			SendKey(UnManagedMethods.VK_DOWN, 0x50); // 
		}

		public void SendUp()
		{
			SendKey(UnManagedMethods.VK_UP, 0x50); // 
		}

		public void SendLeft()
		{
			SendKey(UnManagedMethods.VK_LEFT, 0x50); // 
		}

		public void SendRight()
		{
			SendKey(UnManagedMethods.VK_RIGHT, 0x50); // 
		}

		//public void SendText(char c)
		//{
		//    SendKey(UnManagedMethods.VkKeyScan(c), 0x9e); // ‘A’ Press
		//}

		public WindowStyleFlags WindowStyle
		{
			get
			{
				return (WindowStyleFlags)UnManagedMethods.GetWindowLong(
					this.hWnd, UnManagedMethods.GWL_STYLE);
			}
		}

		public ExtendedWindowStyleFlags ExtendedWindowStyle
		{
			get
			{
				return (ExtendedWindowStyleFlags)UnManagedMethods.GetWindowLong(
					this.hWnd, UnManagedMethods.GWL_EXSTYLE);
			}
		}

		public void Dispose()
		{
			this.hWnd = IntPtr.Zero;
		}
	}
	#endregion
}
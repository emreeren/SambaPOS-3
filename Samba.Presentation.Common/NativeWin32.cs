using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Samba.Presentation.Common
{
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public long time;
        public uint dwExtraInfo;
    };

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public uint type;
        [FieldOffset(4)]
        public KEYBDINPUT ki;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InputKeys
    {
        public uint type;
        public uint wVk;
        public uint wScan;
        public uint dwFlags;
        public uint time;
        public uint dwExtra;
    }

    public class NativeWin32
    {
        //    public const ushort KEYEVENTF_KEYUP = 0x0002;
        public const uint INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWSAMBAPOS = RegisterWindowMessage("WM_SHOWSAMBAPOS");

        [DllImport("user32.dll")]
        public static extern Boolean Keybd_Event(int dwKey, byte bScan, Int32 dwFlags, Int32 dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("User32.DLL", EntryPoint = "SendInput")]
        public static extern uint SendInput(uint nInputs, InputKeys[] inputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

    }


    ///<summary>
    /// Encapsulates the native methods
    ///</summary>
    public static class UnsafeNativeMethods
    {
        #region Nested type: RECT

        ///<summary>
        /// A rectangular windows structure
        ///</summary>
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            ///<summary>
            /// The horizontal position of the left edge
            ///</summary>
            public int Left;

            ///<summary>
            /// The vertical position of the top edge
            ///</summary>
            public int Top;

            /// <summary>
            /// The horizontal position of the right edge
            /// </summary>
            public int Right;

            ///<summary>
            /// The vertical position of the bottom edge
            ///</summary>
            public int Bottom;

            ///<summary>
            /// Default constructor
            ///</summary>
            ///<param name="left"></param>
            ///<param name="top"></param>
            ///<param name="right"></param>
            ///<param name="bottom"></param>
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            ///<summary>
            /// The height of the rectangle
            ///</summary>
            public int Height
            {
                get
                {
                    return Bottom - Top;
                }
            }

            ///<summary>
            /// The Width of the rectangle
            ///</summary>
            public int Width
            {
                get
                {
                    return Right - Left;
                }
            }

            ///<summary>
            /// Size of the rectangle
            ///</summary>
            public Size Size
            {
                get
                {
                    return new Size(Width, Height);
                }
            }

            ///<summary>
            /// Position of the rectangle top-left corner
            ///</summary>
            public Point Location
            {
                get
                {
                    return new Point(Left, Top);
                }
            }


            ///<summary>
            /// Handy method for converting to a System.Drawing.Rectangle
            ///</summary>
            ///<returns></returns>
            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }

            ///<summary>
            /// Convert a rectangle into a RECT
            ///</summary>
            ///<param name="rectangle"></param>
            ///<returns></returns>
            public static RECT FromRectangle(Rectangle rectangle)
            {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                       ^ ((Width << 0x1a) | (Width >> 6))
                       ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            ///<summary>
            /// implicit conversion to Rectangle
            ///</summary>
            ///<param name="rect"></param>
            ///<returns></returns>
            public static implicit operator Rectangle(RECT rect)
            {
                return rect.ToRectangle();
            }

            ///<summary>
            /// implicit conversion to RECT
            ///</summary>
            ///<param name="rect"></param>
            ///<returns></returns>
            public static implicit operator RECT(Rectangle rect)
            {
                return FromRectangle(rect);
            }

            #endregion

            ///<summary>
            /// return true if of the same position and size
            ///</summary>
            ///<param name="rect"></param>
            ///<returns></returns>
            public bool Equals(RECT rect)
            {
                return Top == rect.Top && Left == rect.Left && Bottom == rect.Bottom && Right == rect.Right;
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <returns>
            /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
            /// </returns>
            /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (obj is RECT)
                {
                    return Equals((RECT)obj);
                }
                return base.Equals(obj);
            }
        }

        #endregion

        #region Nested type: WINDOWINFO

        ///<summary>
        /// Windows information structure
        ///</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            ///<summary>
            ///The size of the structure, in bytes. The caller must set this to sizeof(WINDOWINFO). 
            ///</summary>
            public uint cbSize;

            ///<summary>
            ///RECT structure that specifies the coordinates of the window. 
            ///</summary>
            public RECT rcWindow;

            ///<summary>
            ///RECT structure that specifies the coordinates of the client area. 
            ///</summary>
            public RECT rcClient;

            ///<summary>
            ///The window styles
            ///</summary>
            public uint dwStyle;

            /// <summary>
            /// The extended window styles
            /// </summary>
            public uint dwExStyle;

            /// <summary>
            /// The window status. If this member is WS_ACTIVECAPTION, the window is active. Otherwise, this member is zero.
            /// </summary>
            public uint dwWindowStatus;

            /// <summary>
            /// The width of the window border, in pixels. 
            /// </summary>
            public uint cxWindowBorders;

            /// <summary>
            /// The height of the window border, in pixels.
            /// </summary>
            public uint cyWindowBorders;

            /// <summary>
            /// The window class atom 
            /// </summary>
            public ushort atomWindowType;

            /// <summary>
            /// The Microsoft Windows version of the application that created the window
            /// </summary>
            public ushort wCreatorVersion;
        }

        #endregion


        #region Public Methods

        ///<summary>
        ///Sends the specified message to a window or windows. It calls
        ///the window procedure for the specified window and does not
        ///return until the window procedure has processed the message.
        ///</summary>
        ///<param name="hWnd"></param>
        ///<param name="msg"></param>
        ///<param name="wParam"></param>
        ///<param name="lParam"></param>
        ///<returns></returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        ///<summary>
        /// Delegates mousedown events to underlying controls
        ///</summary>
        ///<returns></returns>
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        ///<summary>
        /// retrieves information about the specified window
        ///</summary>
        ///<param name="hwnd"></param>
        ///<param name="pwi"></param>
        ///<returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        /// <summary>
        /// used to get window process ID
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out Int32 lpdwProcessId);

        ///<summary>
        ///used to get window process ID
        ///</summary>
        ///<param name="hwnd"></param>
        ///<returns></returns>
        public static Int32 GetWindowProcessID(IntPtr hwnd)
        {
            Int32 pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        ///<summary>
        ///Changes the size, position, and Z order of a child, pop-up, or top-level window.
        ///</summary>
        ///<param name="hWnd"></param>
        ///<param name="hWndInsertAfter"></param>
        ///<param name="X"></param>
        ///<param name="Y"></param>
        ///<param name="cx"></param>
        ///<param name="cy"></param>
        ///<param name="uFlags"></param>
        ///<returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx,
                                               int cy, int uFlags);

        ///<summary>
        /// Get the active window
        ///</summary>
        ///<returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        ///<summary>
        /// Redraw window (for showing the content of the form when moving it)
        ///</summary>
        ///<param name="m"></param>
        public static void ReDrawWindow(System.Windows.Forms.Message m)
        {
            RECT rectangle = (RECT)Marshal.PtrToStructure(
                                        m.LParam, typeof(RECT));
            SetWindowPos(m.HWnd, IntPtr.Zero, rectangle.Left, rectangle.Top, rectangle.Width,
                         rectangle.Height, SWP_NoActivate | SWP_ShowWindow | SWP_NoSendChanging);
        }

        #endregion

        #region Properties

        ///<summary>
        ///The HT_CAPTION flag tells the message that the click
        ///occurred in the caption area (the titlebar)
        ///</summary>
        public static int HT_CAPTION
        {
            get
            {
                return 0x2;
            }
        }

        ///<summary>
        ///The WM_NCLBUTTONDOWN message is posted when the user presses
        ///the left mouse button while the cursor is within the
        ///nonclient area of a window
        ///</summary>
        public static int WM_NCLBUTTONDOWN
        {
            get
            {
                return 0xA1;
            }
        }

        ///<summary>
        /// Specifies that a window created with this style should be placed above all 
        /// nontopmost windows and stay above them even when the window is deactivated. 
        ///</summary>
        public static int WS_EX_TopMost
        {
            get
            {
                return 0x00000008;
            }
        }

        ///<summary>
        /// The flag tells Windows Xp/2000+ that the window is a layered window
        ///</summary>
        public static int WS_EX_Layered
        {
            get
            {
                return 0x80000;
            }
        }

        /// <summary>
        /// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. 
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// </summary>
        public static int WS_EX_NoActivate
        {
            get
            {
                return 0x08000000;
            }
        }


        /// <summary>
        /// The flag declares the window as a tool window, therefore it
        /// does not appear in the Alt-Tab application list
        /// </summary>
        public static int WS_EX_ToolWindow
        {
            get
            {
                return 0x80;
            }
        }

        /// <summary>
        /// Allows the windows to be transparent to the mouse.
        /// </summary>
        public static int WS_EX_Transparent
        {
            get
            {
                return 0x20;
            }
        }

        /// <summary>
        /// The WM_MOVING message is sent to a window that the user is moving
        /// </summary>
        public static int WM_MOVING
        {
            get
            {
                return 0x216;
            }
        }

        public static int WM_SIZING
        {
            get
            {
                return 0x214;
            }
        }

        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int MA_NOACTIVATE = 3;
        
        /// <summary>
        /// Does not activate the window. If this flag is not set, the
        /// window is activated and moved to the top of either the
        /// topmost or non-topmost group .
        /// </summary>
        public static int SWP_NoActivate
        {
            get
            {
                return 0x0010;
            }
        }

        /// <summary>
        /// Displays the window
        /// </summary>
        public static int SWP_ShowWindow
        {
            get
            {
                return 0x0040;
            }
        }

        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message
        /// </summary>
        public static int SWP_NoSendChanging
        {
            get
            {
                return 0x0400;
            }
        }

        #endregion
    }


}


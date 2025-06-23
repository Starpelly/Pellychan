using Pellychan.GUI.Platform.SDL3;
using Pellychan.GUI.Platform.Windows.Native;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static SDL.SDL3;

namespace Pellychan.GUI.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    internal class SDL3WindowsWindow : SDL3Window
    {
        private const int seticon_message = 0x0080;
        private const int icon_big = 1;
        private const int icon_small = 0;

        private const int large_icon_size = 256;
        private const int small_icon_size = 16;

        private Icon? m_smallIcon;
        private Icon? m_largeIcon;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        const int GCL_STYLE = -26;
        const int CS_DROPSHADOW = 0x00020000;

        /// <summary>
        /// On Windows, SDL will use the same image for both large and small icons (scaled as necessary).
        /// This can look bad if scaling down a large image, so we use the Windows API directly so as
        /// to get a cleaner icon set than SDL can provide.
        /// If called before the window has been created, or we do not find two separate icon sizes, we fall back to the base method.
        /// </summary>
        internal override void SetIconFromGroup(IconGroup iconGroup)
        {
            m_smallIcon = iconGroup.CreateIcon(small_icon_size, small_icon_size);
            m_largeIcon = iconGroup.CreateIcon(large_icon_size, large_icon_size);

            IntPtr windowHandle = WindowHandle;

            if (windowHandle == IntPtr.Zero || m_largeIcon == null || m_smallIcon == null)
                base.SetIconFromGroup(iconGroup);
            else
            {
                SetIconNative(m_smallIcon, m_largeIcon);
            }
        }

        internal void SetIconNative(Icon smallIcon, Icon bigIcon)
        {
            SendMessage(WindowHandle, seticon_message, icon_small, smallIcon.Handle);
            SendMessage(WindowHandle, seticon_message, icon_big, bigIcon.Handle);
        }

        internal override void CopyIconFromOther(SDL3Window other)
        {
            if (other is not SDL3WindowsWindow window)
                throw new Exception("How did you do this?");

            if (window.m_smallIcon == null || window.m_largeIcon == null)
                return;

            SetIconNative(window.m_smallIcon, window.m_largeIcon);
        }

        internal void ResetDropShadow()
        {
            DisableDropShadowViaClassStyle(WindowHandle);
        }

        internal void AddDropShadow()
        {
            TryEnableDropShadowViaClassStyle(WindowHandle);
            // AddDropShadow(WindowHandle);
        }

        static void AddDropShadow(IntPtr hwnd)
        {
            if (DwmIsCompositionEnabled(out bool enabled) == 0 && enabled)
            {
                var margins = new MARGINS
                {
                    cxLeftWidth = 1,
                    cxRightWidth = 1,
                    cyTopHeight = 1,
                    cyBottomHeight = 1
                };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }

        static nint OriginalWindowStylePTR;

        static void TryEnableDropShadowViaClassStyle(IntPtr hwnd)
        {
            if (OriginalWindowStylePTR == 0)
                OriginalWindowStylePTR = GetClassLongPtr(hwnd, GCL_STYLE);

            SetClassLongPtr(hwnd, GCL_STYLE, (IntPtr)(OriginalWindowStylePTR.ToInt64() | CS_DROPSHADOW));
        }

        static void DisableDropShadowViaClassStyle(IntPtr hwnd)
        {
            if (OriginalWindowStylePTR == 0)
                return;

            SetClassLongPtr(hwnd, GCL_STYLE, OriginalWindowStylePTR);
        }
    }
}

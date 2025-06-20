using Pellychan.GUI.Platform.SDL3;
using Pellychan.GUI.Platform.Windows.Native;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

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
                SendMessage(windowHandle, seticon_message, icon_small, m_smallIcon.Handle);
                SendMessage(windowHandle, seticon_message, icon_big, m_largeIcon.Handle);
            }
        }
    }
}

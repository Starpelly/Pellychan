using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Pellychan.GUI.Platform.Windows.Native;

[SupportedOSPlatform("windows")]
internal class Icon : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private bool m_disposed = false;

    public IntPtr Handle { get; private set; }

    public readonly int Width;
    public readonly int Height;

    internal Icon(IntPtr handle, int width, int height)
    {
        Handle = handle;
        Width = width;
        Height = height;
    }

    ~Icon()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        if (Handle != IntPtr.Zero)
        {
            DestroyIcon(Handle);
            Handle = IntPtr.Zero;
        }

        m_disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
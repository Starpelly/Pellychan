using SkiaSharp;
using System.Runtime.InteropServices;

namespace Pellychan.GUI.Framework.Platform.Skia;

public static class SkiaHelper
{
    /*
    public static GRContext GenerateSkiaContext(Window nativeWindow)
    {
        var nativeContext = GetNativeContext(nativeWindow);
        // var glInterface = GRGlInterface.AssembleGlInterface(nativeContext, (contextHandle, name) => Glfw.GetProcAddress(name));
        var glInterface = GRGlInterface.Create();
        return GRContext.CreateGl(glInterface);
    }

    public static object GetNativeContext(Window nativeWindow)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Native.GetWglContext(nativeWindow);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // XServer
            return Native.GetGLXContext(nativeWindow);
            // Wayland
            //return Native.GetEglContext(nativeWindow);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Native.GetNSGLContext(nativeWindow);
        }

        throw new PlatformNotSupportedException();
    }
    */
}
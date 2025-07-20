namespace Pellychan.GUI;

public static class Config
{
    /// <summary>
    /// 
    /// </summary>
    public static bool HeadlessMode { get; set; } = false;

    /// <summary>
    /// Tells Skia to render using the GPU.
    /// </summary>
    public static bool HardwareAccel { get; private set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public const bool SHARE_GL_CONTEXTS = false;
    
    /// <summary>
    /// When popups are created, they make native OS windows to handle them.
    /// This also creates separate GL contexts.
    /// </summary>
    public const bool POPUPS_MAKE_WINDOWS = false;

    /// <summary>
    /// 
    /// </summary>
    public const bool SUPPORT_PAINT_CACHING = false;
}
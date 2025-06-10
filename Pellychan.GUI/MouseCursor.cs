using SDL2;

namespace Pellychan.GUI;

public static class MouseCursor
{
    private static IntPtr _currentCursor = IntPtr.Zero;

    public enum CursorType
    {
        Arrow,
        IBeam,
        Wait,
        Crosshair,
        WaitArrow,
        SizeNWSE,
        SizeNESW,
        SizeWE,
        SizeNS,
        SizeAll,
        No,
        Hand
    }

    public static void Set(CursorType type)
    {
        // Free the old cursor if any
        if (_currentCursor != IntPtr.Zero)
        {
            SDL.SDL_FreeCursor(_currentCursor);
            _currentCursor = IntPtr.Zero;
        }

        // Create the new system cursor
        SDL.SDL_SystemCursor systemCursor = type switch
        {
            CursorType.Arrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            CursorType.IBeam => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
            CursorType.Wait => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            CursorType.Crosshair => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            CursorType.WaitArrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAITARROW,
            CursorType.SizeNWSE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE,
            CursorType.SizeNESW => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW,
            CursorType.SizeWE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
            CursorType.SizeNS => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
            CursorType.SizeAll => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
            CursorType.No => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO,
            CursorType.Hand => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW
        };

        _currentCursor = SDL.SDL_CreateSystemCursor(systemCursor);
        SDL.SDL_SetCursor(_currentCursor);
    }

    public static void Reset()
    {
        Set(CursorType.Arrow);
    }

    public static void Cleanup()
    {
        if (_currentCursor != IntPtr.Zero)
        {
            SDL.SDL_FreeCursor(_currentCursor);
            _currentCursor = IntPtr.Zero;
        }
    }
}
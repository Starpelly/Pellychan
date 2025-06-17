using SDL;

namespace Pellychan.GUI;

public static unsafe class MouseCursor
{
    private static SDL_Cursor* _currentCursor;

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
        if (_currentCursor != null)
        {
            SDL3.SDL_DestroyCursor(_currentCursor);
            _currentCursor = null;
        }

        // Create the new system cursor
        SDL.SDL_SystemCursor systemCursor = type switch
        {
            CursorType.Arrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT,
            CursorType.IBeam => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_TEXT,
            CursorType.Wait => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            CursorType.Crosshair => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            CursorType.WaitArrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_PROGRESS,
            CursorType.SizeNWSE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NWSE_RESIZE,
            CursorType.SizeNESW => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NESW_RESIZE,
            CursorType.SizeWE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_EW_RESIZE,
            CursorType.SizeNS => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NS_RESIZE,
            CursorType.SizeAll => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_MOVE,
            CursorType.No => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NOT_ALLOWED,
            CursorType.Hand => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_POINTER,
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_DEFAULT
        };

        _currentCursor = SDL3.SDL_CreateSystemCursor(systemCursor);
        SDL3.SDL_SetCursor(_currentCursor);
    }

    public static void Reset()
    {
        Set(CursorType.Arrow);
    }

    public static void Cleanup()
    {
        if (_currentCursor != null)
        {
            SDL3.SDL_DestroyCursor(_currentCursor);
            _currentCursor = null;
        }
    }
}
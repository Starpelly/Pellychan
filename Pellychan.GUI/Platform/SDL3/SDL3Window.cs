using Pellychan.GUI.Utils;
using SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SDL.SDL3;

namespace Pellychan.GUI.Platform.SDL3
{
    internal abstract unsafe partial class SDL3Window : IWindow
    {
        internal SDL_Window* SDLWindowHandle { get; private set; } = null;
        internal SDL_WindowID SDLWindowID { get; private set; }

        /// <summary>
        /// Returns true if the window has been created.
        /// Returns false if the window has not yet been created, or has been closed.
        /// </summary>
        public bool Exists { get; private set; }

        private string m_title = string.Empty;

        /// <summary>
        /// Gets and sets the window title.
        /// </summary>
        public string Title
        {
            get => m_title;
            set
            {
                m_title = value;
                SDL_SetWindowTitle(SDLWindowHandle, m_title);
            }
        }

        /// <summary>
        /// Whether the current display server is Wayland.
        /// </summary>
        internal bool IsWayland => SDL_GetCurrentVideoDriver() == "wayland";

        /// <summary>
        /// Gets the native window handle as provided by the operating system.
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                if (SDLWindowHandle == null)
                    return IntPtr.Zero;

                var props = SDL_GetWindowProperties(SDLWindowHandle);

                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Windows:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.Linux:
                        if (IsWayland)
                            return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, IntPtr.Zero);

                        if (SDL_GetCurrentVideoDriver() == "x11")
                            return new IntPtr(SDL_GetNumberProperty(props, SDL_PROP_WINDOW_X11_WINDOW_NUMBER, 0));

                        return IntPtr.Zero;

                    case RuntimeInfo.Platform.macOS:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_COCOA_WINDOW_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.iOS:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER, IntPtr.Zero);

                    case RuntimeInfo.Platform.Android:
                        return SDL_GetPointerProperty(props, SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER, IntPtr.Zero);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Represents a handle to this <see cref="SDL3Window"/> instance, used for unmanaged callbacks.
        /// </summary>
        protected ObjectHandle<SDL3Window> ObjectHandle { get; private set; }

        protected SDL3Window()
        {
            ObjectHandle = new ObjectHandle<SDL3Window>(this, GCHandleType.Normal);

            SDL_SetLogPriority(SDL_LogCategory.SDL_LOG_CATEGORY_ERROR, SDL_LogPriority.SDL_LOG_PRIORITY_DEBUG);
            SDL_SetEventFilter(&eventFilter, ObjectHandle.Handle);
        }

        public void Create()
        {
            SDL_WindowFlags flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                                    SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY |
                                    SDL_WindowFlags.SDL_WINDOW_HIDDEN;

            SDLWindowHandle = SDL_CreateWindow(m_title, Size.Width, Size.Height, flags);
            SDLWindowID = SDL_GetWindowID(SDLWindowHandle);

            Exists = true;

            SDL_AddEventWatch(&eventWatch, ObjectHandle.Handle);
        }

        public void Close()
        {
            Exists = false;
        }

        public void Show()
        {
            SDL_ShowWindow(SDLWindowHandle);
        }

        private const int events_per_peep = 64;
        private readonly SDL_Event[] events = new SDL_Event[events_per_peep];

        /// <summary>
        /// Poll for all pending events.
        /// </summary>
        public void pollSDLEvents()
        {
            SDL_PumpEvents();

            int eventsRead;

            do
            {
                eventsRead = SDL_PeepEvents(events, SDL_EventAction.SDL_GETEVENT, SDL_EventType.SDL_EVENT_FIRST, SDL_EventType.SDL_EVENT_LAST);
                for (int i = 0; i < eventsRead; i++)
                    HandleEvent(events[i]);
            } while (eventsRead == events_per_peep);
        }

        protected void HandleEvent(SDL_Event e)
        {
            if (e.Type >= SDL_EventType.SDL_EVENT_WINDOW_FIRST && e.Type <= SDL_EventType.SDL_EVENT_WINDOW_LAST)
            {
                handleWindowEvent(e.window);
                return;
            }

            switch (e.Type)
            {
                case SDL_EventType.SDL_EVENT_QUIT:
                    ExitRequested?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Handles <see cref="SDL_Event"/>s fired from the SDL event filter.
        /// </summary>
        /// <remarks>
        /// As per SDL's recommendation, application events should always be handled via the event filter.
        /// See: https://wiki.libsdl.org/SDL3/SDL_EventType#android_ios_and_winrt_events
        /// </remarks>
        /// <returns>A <c>bool</c> denoting whether to keep the event. <c>false</c> will drop the event.</returns>
        protected virtual bool HandleEventFromFilter(SDL_Event e)
        {
            switch (e.Type)
            {
                case SDL_EventType.SDL_EVENT_TERMINATING:
                    ExitRequested?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_DID_ENTER_BACKGROUND:
                    // Suspended?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_WILL_ENTER_FOREGROUND:
                    // Resumed?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_LOW_MEMORY:
                    // LowOnMemory?.Invoke();
                    break;

                case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    handleMouseMotionEvent(e.motion);
                    return false;

                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    handleMouseButtonEvent(e.button);
                    return false;

                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    handleMouseWheelEvent(e.wheel);
                    return false;
            }

            return true;
        }


        protected void HandleEventFromWatch(SDL_Event evt)
        {
            switch (evt.Type)
            {
                case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                    // polling via SDL_PollEvent blocks on resizes (https://stackoverflow.com/a/50858339)
                    if (!m_updatingWindowStateAndSize)
                    {
                        // bool isUserResizing = SDL_GetGlobalMouseState(null, null).HasFlag(SDL_MouseButtonFlags.SDL_BUTTON_LMASK);
                        fetchWindowSize();
                    }
                    break;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static SDLBool eventFilter(IntPtr userdata, SDL_Event* eventPtr)
        {
            var handle = new ObjectHandle<SDL3Window>(userdata);
            if (handle.GetTarget(out SDL3Window window))
                return window.HandleEventFromFilter(*eventPtr);

            return true;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static SDLBool eventWatch(IntPtr userdata, SDL_Event* eventPtr)
        {
            var handle = new ObjectHandle<SDL3Window>(userdata);
            if (handle.GetTarget(out SDL3Window window))
                window.HandleEventFromWatch(*eventPtr);

            return true;
        }

        #region Events

        /// <summary>
        /// Invoked when the window close (X) button or another platform-native exit action has been pressed.
        /// </summary>
        public event Action? ExitRequested;

        /// <summary>
        /// Invoked when the window is about to close.
        /// </summary>
        public event Action? Exited;

        #endregion

        public void Dispose()
        {
            Close();

            ObjectHandle.Dispose();
        }
    }
}

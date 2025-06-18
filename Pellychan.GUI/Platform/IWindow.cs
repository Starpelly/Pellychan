using Pellychan.GUI.Platform.Input;
using System.Drawing;
using System.Numerics;

namespace Pellychan.GUI.Platform
{
    internal interface IWindow : IDisposable
    {
        /// <summary>
        /// Creates the concrete window implementation.
        /// </summary>
        void Create();

        /// <summary>
        /// Forcefully closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Attempts to show the window, making it visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Invoked when the window close (X) button or another platform-native exit action has been pressed.
        /// </summary>
        event Action? ExitRequested;

        /// <summary>
        /// Invoked when the <see cref="IWindow"/> has closed.
        /// </summary>
        event Action? Exited;

        /// <summary>
        /// Invoked when the <see cref="IWindow"/> client size has changed.
        /// </summary>
        event Action? Resized;

        /// <summary>
        /// Invoked when the user moves the mouse cursor within the window.
        /// </summary>
        event Action<Vector2> MouseMove;

        /// <summary>
        /// Invoked when the user presses a mouse button.
        /// </summary>
        event Action<Vector2, MouseButton> MouseDown;

        /// <summary>
        /// Invoked when the user releases a mouse button.
        /// </summary>
        event Action<Vector2, MouseButton> MouseUp;

        /// <summary>
        /// Invoked when the user scrolls the mouse wheel over the window.
        /// </summary>
        /// <remarks>
        /// Delta is positive when mouse wheel scrolled to the up or left, in non-"natural" scroll mode (ie. the classic way).
        /// </remarks>
        public event Action<Vector2, bool> MouseWheel;

        /// <summary>
        /// Controls the state of the window.
        /// </summary>
        WindowState WindowState { get; set; }

        /// <summary>
        /// Invoked when <see cref="WindowState"/> changes.
        /// </summary>
        event Action<WindowState> WindowStateChanged;

        /// <summary>
        /// Invoked when the window moves.
        /// </summary>
        public event Action<Point> Moved;

        /// <summary>
        /// The client size of the window in pixels (excluding any window decoration/border).
        /// </summary>
        Size ClientSize { get; }

        /// <summary>
        /// The position of the window.
        /// </summary>
        Point Position { get; }

        /// <summary>
        /// The size of the window in scaled pixels (excluding any window decoration/border).
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// The ratio of <see cref="ClientSize"/> and <see cref="Size"/>.
        /// </summary>
        float Scale { get; }

        /// <summary>
        /// The minimum size of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when setting a negative size, or a size greater than <see cref="MaxSize"/>.</exception>
        Size MinSize { get; set; }

        /// <summary>
        /// The maximum size of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when setting a negative or zero size, or a size less than <see cref="MinSize"/>.</exception>
        Size MaxSize { get; set; }

        /// <summary>
        /// Gets or sets whether the window is user-resizable.
        /// </summary>
        bool Resizable { get; set; }

        /// <summary>
        /// The window title.
        /// </summary>
        string Title { get; set; }
    }
}

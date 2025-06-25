using Pellychan.GUI.Input;

namespace Pellychan.GUI.Widgets;

public partial class Widget
{
    private void handleMouseEnter()
    {
        if (!m_hovered)
        {
            m_hovered = true;
            (this as IMouseEnterHandler)?.OnMouseEnter();
        }
    }

    private void handleMouseLeave()
    {
        if (m_hovered)
        {
            m_hovered = false;
            (this as IMouseLeaveHandler)?.OnMouseLeave();
        }
    }

    private void onNativeWindowMouseEvent(int mouseX, int mouseY, MouseEventType type, int deltaX = 0, int deltaY = 0)
    {
        var hovered = findHoveredWidget(mouseX, mouseY, true);

        if (hovered != m_lastHovered)
        {
            m_lastHovered?.handleMouseLeave();
            hovered?.handleMouseEnter();
            m_lastHovered = hovered;
        }

        // Console.WriteLine($"{this.Name}, {hovered?.Name}, ({mouseX}, {mouseY})");

        // If there's a mouse grabber, it always receives input!
        if (s_mouseGrabber != null && s_mouseGrabber.Enabled)
        {
            var (lx, ly) = getLocalPosition(s_mouseGrabber, mouseX, mouseY);

            switch (type)
            {
                case MouseEventType.Move:
                    (s_mouseGrabber as IMouseMoveHandler)?.OnMouseMove(lx, ly);
                    break;

                case MouseEventType.Down:
                    (s_mouseGrabber as IMouseDownHandler)?.OnMouseDown(lx, ly);
                    break;

                case MouseEventType.Up:
                    (s_mouseGrabber as IMouseUpHandler)?.OnMouseUp(lx, ly);

                    if (s_mouseGrabber == hovered)
                    {
                        (s_mouseGrabber as IMouseClickHandler)?.OnMouseClick(lx, ly);
                    }

                    s_mouseGrabber = null;
                    break;

                case MouseEventType.Wheel:
                    (s_mouseGrabber as IMouseWheelHandler)?.OnMouseScroll(lx, ly, deltaX, deltaY);
                    break;
            }

            return;
        }

        // No grabber - do regular hit testing.
        if (hovered != null)
        {
            switch (type)
            {
                case MouseEventType.Down:
                    if (bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY))
                    {
                        s_mouseGrabber = hovered;
                    }
                    break;

                case MouseEventType.Up:
                    bool upHandled = bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY);

                    if (s_mouseGrabber == hovered && upHandled)
                    {
                        var (lx, ly) = getLocalPosition(hovered, mouseX, mouseY);
                        (hovered as IMouseClickHandler)?.OnMouseClick(lx, ly);
                    }

                    s_mouseGrabber = null;
                    break;

                default:
                    bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY);
                    break;
            }
        }
    }

    private bool bubbleMouseEvent(Widget widget, MouseEventType type, int globalX, int globalY, int dx = 0, int dy = 0)
    {
        while (widget != null)
        {
            if (!widget.Enabled)
            {
                widget = widget.Parent!;
                continue;
            }

            var (lx, ly) = getLocalPosition(widget, globalX, globalY);

            bool handled = type switch
            {
                MouseEventType.Move => (widget as IMouseMoveHandler)?.OnMouseMove(lx, ly) ?? false,
                MouseEventType.Down => (widget as IMouseDownHandler)?.OnMouseDown(lx, ly) ?? false,
                MouseEventType.Up => (widget as IMouseUpHandler)?.OnMouseUp(lx, ly) ?? false,
                MouseEventType.Wheel => (widget as IMouseWheelHandler)?.OnMouseScroll(lx, ly, dx, dy) ?? false,
                _ => false
            };

            if (handled) return true;
            if (widget.IsWindow) return false;

            widget = widget.Parent!;
        }

        return false;
    }

    private void onNativeWindowResizeEvent(int w, int h)
    {
        {
            m_width = w;
            m_height = h;

            // This is fine because a native window can only exist on top level widgets and thus,
            // can't be in a layout!
            if (m_nativeWindow != null)
            {
                m_nativeWindow.Window.Size = new System.Drawing.Size(m_width, m_height);
            }

            dispatchResize();
            CallResizeEvents();
        }
        m_nativeWindow!.CreateFrameBuffer(w, h);

        TriggerRepaint();

        LayoutQueue.Flush();
        RenderTopLevel(Application.DebugDrawing);
    }
}
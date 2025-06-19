using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;

namespace Pellychan.GUI;

internal static class LayoutQueue
{
    private static readonly HashSet<DirtyWidget> s_dirtyWidgets = [];
    public static bool IsFlusing { get; private set; } = false;

    private const bool LogChanges = false;

    private struct DirtyWidget
    {
        public LayoutFlushType FlushType;
        public Widget Widget;
    }

    public static void Enqueue(Widget widget, LayoutFlushType flushType)
    {
        // Why would this be the case? Idk...
        if (widget == null)
            return;
        if (widget.Layout == null)
            return;

        var oldCount = s_dirtyWidgets.Count;

        s_dirtyWidgets.Add(new()
        {
            Widget = widget,
            FlushType = flushType
        });

        if (s_dirtyWidgets.Count > oldCount && LogChanges)
            Console.WriteLine($"Enqued: {widget.Name}");
    }

    public static void Flush()
    {
        IsFlusing = true;

        bool hadWorkAll = s_dirtyWidgets.Count > 0;
        if (hadWorkAll && LogChanges)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================Started flush!========================");
            Console.ResetColor();
        }

        while (true)
        {
            bool hadWork = s_dirtyWidgets.Count > 0;
            if (hadWork && LogChanges)
                Console.WriteLine("------------------Layout Flush Start------------------");

            var toWork = new DirtyWidget[s_dirtyWidgets.Count];
            lock (s_dirtyWidgets)
            {
                if (s_dirtyWidgets.Count == 0)
                    break;

                s_dirtyWidgets.CopyTo(toWork);
                s_dirtyWidgets.Clear();
            }

            foreach (var dirty in toWork)
            {
                // Sometimes this can be null? I don't know how or why
                // but I guess we'll handle it in that case???
                dirty.Widget?.PerformLayoutUpdate(dirty.FlushType);
            }

            if (hadWork && LogChanges)
                Console.WriteLine("------------------Layout Flush End------------------");
        }
        IsFlusing = false;

        if (hadWorkAll && LogChanges)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================Ended flush!=========================");
            Console.ResetColor();
        }
    }
}
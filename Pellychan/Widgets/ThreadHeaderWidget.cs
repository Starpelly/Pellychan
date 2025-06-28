using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pellychan.Widgets;

public class ThreadHeaderWidget : Widget, IPaintHandler
{
    private API.Models.Post m_post;
    private bool m_loadedPost = false;

    public ThreadHeaderWidget(Widget? parent = null) : base(parent)
    {
        Fitting = new(GUI.Layouts.FitPolicy.Policy.Expanding, GUI.Layouts.FitPolicy.Policy.Fixed);
        Height = 165;
    }

    public void OnPaint(SKCanvas canvas)
    {
        if (m_loadedPost == false)
            return;

        using var paint = new SKPaint();

        if (!string.IsNullOrEmpty(m_post.Sub))
        {
            canvas.DrawText(m_post.Sub, new SKPoint(46, 55), Application.DefaultFontBold, paint);
        }
    }

    #region Public methods

    public void SetPost(API.Models.Post post)
    {
        m_post = post;
        m_loadedPost = true;
    }

    #endregion
}
using SkiaSharp;
using System.Diagnostics;
using static Pellychan.ChanClient;

namespace Pellychan;

public class GifPlayer : IDisposable
{
    private List<GifFrame> m_frames = [];
    private int m_currentFrame = 0;
    private double m_nextFrameTime = 0;
    private readonly Stopwatch m_stopwatch = new();

    public SKImage? CurrentImage => m_frames.Count > 0 ? m_frames[m_currentFrame].Image : null;

    public async Task LoadAsync(string url, Action onComplete)
    {
        m_frames = await Pellychan.ChanClient.LoadGifFromUrlAsync(url);

        if (m_frames.Count > 0)
        {
            Start();
        }

        onComplete.Invoke();
    }

    public void Start()
    {
        StartTimer();
    }

    public void Stop()
    {
        m_currentFrame = 0;
        m_stopwatch.Stop();
    }

    public void Update()
    {
        if (m_frames.Count == 0) return;
        while (m_stopwatch.Elapsed.TotalSeconds >= m_nextFrameTime)
        {
            m_currentFrame = (m_currentFrame + 1) % m_frames.Count;
            m_nextFrameTime += m_frames[m_currentFrame].Delay * 0.001;
            OnFrameChanged?.Invoke();
        }
    }

    private void StartTimer()
    {
        m_stopwatch.Stop();
        m_stopwatch.Start();
    }

    public Action? OnFrameChanged { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var frame in m_frames)
            frame.Image.Dispose();
        m_frames.Clear();
    }
}
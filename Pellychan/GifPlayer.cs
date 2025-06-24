using SkiaSharp;
using static Pellychan.ChanClient;

namespace Pellychan;

public class GifPlayer : IDisposable
{
    private List<GifFrame> m_frames = [];
    private int m_currentFrame = 0;
    private Timer? m_timer; // DISGUSTING, WE SHOULD USE A STOPWATCH INSTEAD!!!

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
        m_currentFrame = 0;
        StartTimer(m_frames[m_currentFrame].Delay);
    }

    public void Stop()
    {
        m_timer?.Dispose();
    }

    private void StartTimer(int interval)
    {
        m_timer?.Dispose();
        m_timer = new Timer(_ =>
        {
            m_currentFrame = (m_currentFrame + 1) % m_frames.Count;
            StartTimer(m_frames[m_currentFrame].Delay);
            OnFrameChanged?.Invoke(); // hook to trigger repaint
        }, null, interval, Timeout.Infinite);
    }

    public Action? OnFrameChanged { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        m_timer?.Dispose();
        foreach (var frame in m_frames)
            frame.Image.Dispose();
        m_frames.Clear();
    }
}
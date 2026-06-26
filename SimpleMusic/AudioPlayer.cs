using NAudio.Wave;

namespace SimpleMusic;

public class AudioPlayer : IDisposable
{
    private IWavePlayer? _waveOut;
    private AudioFileReader? _fileReader;
    private System.Windows.Forms.Timer? _progressTimer;
    private bool _isStopping;

    public event Action<TimeSpan, TimeSpan>? ProgressChanged;
    public event Action? PlaybackFinished;

    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

    public void Play(string filePath)
    {
        Stop();

        _fileReader = new AudioFileReader(filePath);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_fileReader);
        _waveOut.PlaybackStopped += (s, e) =>
        {
            if (!_isStopping)
                PlaybackFinished?.Invoke();
        };
        _waveOut.Play();

        _progressTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _progressTimer.Tick += (s, e) =>
            ProgressChanged?.Invoke(_fileReader.CurrentTime, _fileReader.TotalTime);
        _progressTimer.Start();
    }

    public void Pause() => _waveOut?.Pause();
    public void Resume() => _waveOut?.Play();

    public void Stop()
    {
        _isStopping = true;
        _progressTimer?.Stop();
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _fileReader?.Dispose();
        _waveOut = null;
        _fileReader = null;
        _isStopping = false;
    }

    public void Seek(double percent)
    {
        if (_fileReader == null) return;
        var pos = _fileReader.TotalTime.TotalSeconds * percent;
        _fileReader.CurrentTime = TimeSpan.FromSeconds(pos);
    }

    public void Dispose() => Stop();
}
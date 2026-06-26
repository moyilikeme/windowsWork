using NAudio.Wave;
namespace SimpleMusic;
/// 音频播放器类，封装 NAudio 库实现音频文件的播放、暂停、恢复、停止和进度控制功能。
/// 实现了 IDisposable 接口，确保非托管资源（音频设备和文件流）能够被正确释放。
public class AudioPlayer : IDisposable
{
    // NAudio 音频输出设备接口，负责将解码后的音频数据输出到声卡
    private IWavePlayer? _waveOut;

    // NAudio 音频文件读取器，负责从音频文件中读取数据并解码
    private AudioFileReader? _fileReader;

    // Windows Forms 定时器，用于定期触发进度更新事件（每100毫秒）
    private System.Windows.Forms.Timer? _progressTimer;

    // 标志位，用于区分用户主动停止还是播放自然结束
    private bool _isStopping;
 
    public event Action<TimeSpan, TimeSpan>? ProgressChanged;
    /// 播放完成事件， 当音频文件自然播放完毕时触发。
    public event Action? PlaybackFinished; 
    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;
   
    /// 播放指定路径的音频文件。
    public void Play(string filePath)
    {
        // 先停止当前正在播放的音频，释放之前的资源，避免内存泄漏
        Stop();

        _fileReader = new AudioFileReader(filePath);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_fileReader);
        _waveOut.PlaybackStopped += (s, e) =>
        {
            // 只有在非用户主动停止的情况下，才触发 PlaybackFinished 事件
            if (!_isStopping)
                PlaybackFinished?.Invoke();
        };
        _waveOut.Play();
        _progressTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _progressTimer.Tick += (s, e) =>
            ProgressChanged?.Invoke(_fileReader.CurrentTime, _fileReader.TotalTime);
        _progressTimer.Start();
    }
    /// 暂停当前播放的音频。
    public void Pause() => _waveOut?.Pause();
    /// 恢复暂停的音频播放。
    public void Resume() => _waveOut?.Play();
    /// 停止当前播放的音频，并释放相关资源。
    public void Stop()
    {       
        _isStopping = true;  
        _progressTimer?.Stop();
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _fileReader?.Dispose();
        _waveOut = null;
        _fileReader = null;

        // 重置停止标志，为下一次播放做准备
        _isStopping = false;
    }    
    /// 根据百分比跳转到音频的指定位置。
    public void Seek(double percent)
    {
        if (_fileReader == null) return;
        // 计算目标位置的秒数：总时长 × 百分比
        var pos = _fileReader.TotalTime.TotalSeconds * percent;
        _fileReader.CurrentTime = TimeSpan.FromSeconds(pos);
    }
    public void Dispose() => Stop();
}
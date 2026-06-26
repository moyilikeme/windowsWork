using SimpleMusic.Models;
using System.Drawing.Drawing2D;

namespace SimpleMusic;

public partial class MainForm : Form
{
    private AudioPlayer _player = new();
    private List<Song> _playlist = new();
    private int _currentIndex = -1;
    private List<LyricLine> _lyrics = new();

    // 颜色配置
    private Color _bgColor = Color.FromArgb(30, 30, 30);
    private Color _accentColor = Color.FromArgb(29, 185, 84);      // 绿色
    private Color _textColor = Color.FromArgb(255, 255, 255);
    private Color _subTextColor = Color.FromArgb(179, 179, 179);
    private Color _hoverColor = Color.FromArgb(40, 40, 40);

    // 控件
    private PictureBox picCover;
    private Label lblTitle;
    private Label lblArtist;
    private Label lblTime;
    private Button btnPrev;
    private Button btnPlay;
    private Button btnNext;
    private ProgressSlider progressSlider;
    private ListBox listBoxSongs;
    private LyricView lyricView;
    private Button btnScan;

    public MainForm()
    {
        InitializeComponent();
        _player.ProgressChanged += OnProgressChanged;
        _player.PlaybackFinished += OnSongFinished;
        this.Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            using var db = new MusicDb("localhost", "simplemusic", "root", "123456");
            _playlist = db.GetAllSongs();
            UpdateList();
        }
        catch
        {
            // 数据库不可用时静默启动，歌曲列表为空
        }
    }

    private void InitializeComponent()
    {
        // ========== 窗体设置 ==========
        this.Text = "SimpleMusic";
        this.Size = new Size(900, 580);
        this.MinimumSize = new Size(900, 580);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = _bgColor;
        this.FormBorderStyle = FormBorderStyle.None;

        // 允许拖动窗体
        this.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Capture = false;
                Message m = Message.Create(this.Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                this.WndProc(ref m);
            }
        };

        // ========== 标题栏 ==========
        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.FromArgb(20, 20, 20)
        };
        this.Controls.Add(titleBar);

        var lblTitleBar = new Label
        {
            Text = "  ■  SimpleMusic",
            ForeColor = _textColor,
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 8)
        };
        titleBar.Controls.Add(lblTitleBar);

        // 关闭按钮
        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(35, 35),
            Location = new Point(this.Width - 45, 0),
            FlatStyle = FlatStyle.Flat,
            ForeColor = _textColor,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
        btnClose.Click += (s, e) => this.Close();
        titleBar.Controls.Add(btnClose);

        // 最小化按钮
        var btnMin = new Button
        {
            Text = "─",
            Size = new Size(35, 35),
            Location = new Point(this.Width - 80, 0),
            FlatStyle = FlatStyle.Flat,
            ForeColor = _textColor,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnMin.FlatAppearance.BorderSize = 0;
        btnMin.FlatAppearance.MouseOverBackColor = _hoverColor;
        btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
        titleBar.Controls.Add(btnMin);

        // ========== 左右分隔线 ==========
        var separator = new Panel
        {
            Location = new Point(430, 35),
            Size = new Size(1, 545),
            BackColor = Color.FromArgb(50, 50, 50)
        };
        this.Controls.Add(separator);

        // ============================================================
        // 左侧：封面 + 歌曲信息 + 控制 + 进度 + 播放列表
        // ============================================================

        // 专辑封面
        picCover = new PictureBox
        {
            Location = new Point(25, 50),
            Size = new Size(150, 150),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(50, 50, 50)
        };
        this.Controls.Add(picCover);

        // 歌曲标题
        lblTitle = new Label
        {
            Location = new Point(190, 55),
            Size = new Size(220, 28),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("微软雅黑", 13, FontStyle.Bold),
            ForeColor = _textColor,
            Text = "未播放"
        };
        this.Controls.Add(lblTitle);

        // 歌手
        lblArtist = new Label
        {
            Location = new Point(190, 88),
            Size = new Size(220, 22),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("微软雅黑", 10),
            ForeColor = _subTextColor,
            Text = "选择歌曲开始播放"
        };
        this.Controls.Add(lblArtist);

        // 时间
        lblTime = new Label
        {
            Location = new Point(190, 116),
            Size = new Size(220, 20),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Consolas", 9),
            ForeColor = _subTextColor,
            Text = "00:00 / 00:00"
        };
        this.Controls.Add(lblTime);

        // 播放控制按钮（水平居中在 430px 宽的左面板中）
        int leftCenter = 215; // 430 / 2
        btnPrev = CreateCircleButton("◀◀", leftCenter - 77, 218);
        btnPlay = CreateCircleButton("▶", leftCenter - 25, 213, true);
        btnNext = CreateCircleButton("▶▶", leftCenter + 35, 218);

        this.Controls.Add(btnPrev);
        this.Controls.Add(btnPlay);
        this.Controls.Add(btnNext);

        // 自定义圆形滑块进度条
        progressSlider = new ProgressSlider
        {
            Location = new Point(25, 265),
            Size = new Size(380, 32),
            BackColor = _bgColor
        };
        this.Controls.Add(progressSlider);

        // 播放列表标题
        var lblPlaylist = new Label
        {
            Text = "  播放列表",
            Location = new Point(25, 308),
            Size = new Size(100, 25),
            ForeColor = _textColor,
            Font = new Font("微软雅黑", 10, FontStyle.Bold)
        };
        this.Controls.Add(lblPlaylist);

        // 播放列表
        listBoxSongs = new ListBox
        {
            Location = new Point(25, 338),
            Size = new Size(380, 165),
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = _textColor,
            BorderStyle = BorderStyle.None,
            Font = new Font("微软雅黑", 9),
            ItemHeight = 28,
            DrawMode = DrawMode.OwnerDrawFixed
        };
        listBoxSongs.DrawItem += ListBoxSongs_DrawItem;
        this.Controls.Add(listBoxSongs);

        // 扫描按钮
        btnScan = new Button
        {
            Text = "🔍 扫描文件夹",
            Location = new Point(155, 513),
            Size = new Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            ForeColor = _textColor,
            BackColor = _hoverColor,
            Font = new Font("微软雅黑", 9),
            Cursor = Cursors.Hand
        };
        btnScan.FlatAppearance.BorderSize = 0;
        btnScan.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
        this.Controls.Add(btnScan);

        // ============================================================
        // 右侧：歌词展示
        // ============================================================

        // 歌词标题
        var lblLyricTitle = new Label
        {
            Text = "♪ 歌词",
            Location = new Point(450, 45),
            Size = new Size(200, 25),
            ForeColor = _textColor,
            Font = new Font("微软雅黑", 11, FontStyle.Bold)
        };
        this.Controls.Add(lblLyricTitle);

        // 多行歌词视图
        lyricView = new LyricView
        {
            Location = new Point(450, 78),
            Size = new Size(425, 470),
            BackColor = Color.FromArgb(35, 35, 35)
        };
        this.Controls.Add(lyricView);

        // ========== 事件绑定 ==========
        btnScan.Click += btnScan_Click;
        listBoxSongs.DoubleClick += listBoxSongs_DoubleClick;
        btnPlay.Click += btnPlay_Click;
        btnPrev.Click += btnPrev_Click;
        btnNext.Click += btnNext_Click;
        progressSlider.Seeked += progressSlider_Seeked;
    }

    // 创建圆形按钮
    private Button CreateCircleButton(string text, int x, int y, bool isLarge = false)
    {
        var size = isLarge ? 50 : 42;
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(size, size),
            FlatStyle = FlatStyle.Flat,
            ForeColor = _textColor,
            BackColor = isLarge ? _accentColor : _hoverColor,
            Font = new Font("微软雅黑", isLarge ? 12 : 9),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;

        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = isLarge ? Color.FromArgb(50, 205, 100) : Color.FromArgb(60, 60, 60);
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = isLarge ? _accentColor : _hoverColor;
        };

        var path = new GraphicsPath();
        path.AddEllipse(0, 0, size, size);
        btn.Region = new Region(path);

        return btn;
    }

    // 自定义列表项绘制
    private void ListBoxSongs_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var song = (Song)listBoxSongs.Items[e.Index];

        var backColor = isSelected ? _accentColor
            : (e.Index % 2 == 0 ? Color.FromArgb(40, 40, 40) : Color.FromArgb(45, 45, 45));
        using (var brush = new SolidBrush(backColor))
        {
            e.Graphics.FillRectangle(brush, e.Bounds);
        }

        // 序号圆圈
        var circleRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 6, 16, 16);
        using (var brush = new SolidBrush(isSelected ? _textColor : _subTextColor))
        {
            e.Graphics.FillEllipse(brush, circleRect);
        }
        using (var brush = new SolidBrush(isSelected ? _accentColor : _bgColor))
        {
            var font = new Font("微软雅黑", 7);
            var text = (e.Index + 1).ToString();
            var size = e.Graphics.MeasureString(text, font);
            e.Graphics.DrawString(text, font, brush,
                circleRect.X + (16 - size.Width) / 2,
                circleRect.Y + (16 - size.Height) / 2);
        }

        // 歌曲名
        using (var brush = new SolidBrush(isSelected ? _bgColor : _textColor))
        {
            var font = new Font("微软雅黑", 9);
            e.Graphics.DrawString(song.Title, font, brush, e.Bounds.X + 35, e.Bounds.Y + 5);
        }

        // 时长
        using (var brush = new SolidBrush(isSelected ? _bgColor : _subTextColor))
        {
            var font = new Font("Consolas", 8);
            var time = song.Duration.ToString(@"mm\:ss");
            var size = e.Graphics.MeasureString(time, font);
            e.Graphics.DrawString(time, font, brush,
                e.Bounds.Right - size.Width - 10, e.Bounds.Y + 7);
        }

        e.DrawFocusRectangle();
    }

    // ========== 业务逻辑 ==========

    private void btnScan_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            btnScan.Enabled = false; // 防止重复点击
            _ = LoadSongsAsync(dlg.SelectedPath); // 丢弃 Task，内部自行处理异常
        }
    }

    /// 异步扫描文件夹、保存数据库并刷新列表。 
    private async Task LoadSongsAsync(string folder)
    {
        try
        {
            _playlist = await new MusicScanner().ScanFolderAsync(folder);

            using var db = new MusicDb("localhost", "simplemusic", "root", "123456");
            db.SaveSongs(_playlist);

            // 跨线程更新 UI
            if (InvokeRequired)
            {
                Invoke(UpdateList);
                Invoke(() => btnScan.Enabled = true);
                return;
            }
            UpdateList();
            btnScan.Enabled = true;
        }
        catch (Exception ex)
        {
            // 异常信息也需 Invoke 到 UI 线程显示
            if (InvokeRequired)
            {
                Invoke(() => MessageBox.Show($"保存到数据库失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error));
                Invoke(() => btnScan.Enabled = true);
                return;
            }
            MessageBox.Show($"保存到数据库失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnScan.Enabled = true;
        }
    }

    /// 刷新列表框数据源。 
    private void UpdateList()
    {
        listBoxSongs.DataSource = null;
        listBoxSongs.DataSource = _playlist;
    }

    /// 双击列表项：切换当前播放。 
    private void listBoxSongs_DoubleClick(object? sender, EventArgs e)
    {
        _currentIndex = listBoxSongs.SelectedIndex;
        PlayCurrent();
    }

    /// 播放当前索引歌曲，更新封面、歌词和 UI。 
    private void PlayCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= _playlist.Count) return;

        var song = _playlist[_currentIndex];
        _player.Play(song.FilePath);

        lblTitle.Text = song.Title;
        lblArtist.Text = song.Artist;
        picCover.Image = LoadCover(song.FilePath);

        // 同步列表选中项
        listBoxSongs.SelectedIndex = _currentIndex;

        // 加载同目录 .lrc 歌词文件
        var lrcPath = Path.ChangeExtension(song.FilePath, ".lrc");
        _lyrics = File.Exists(lrcPath)
            ? new LyricParser().Parse(File.ReadAllText(lrcPath))
            : new List<LyricLine>();

        // 重置进度条和歌词
        progressSlider.Value = 0;
        lyricView.Reset();

        // 更新播放按钮图标
        btnPlay.Text = "⏸";
    }

    /// 加载歌曲封面：优先内嵌标签，其次同目录图片。 
    private Image? LoadCover(string filePath)
    {
        // 1. 先尝试读取音频文件内嵌封面
        try
        {
            var tag = TagLib.File.Create(filePath);
            if (tag.Tag.Pictures.Length > 0)
            {
                var data = tag.Tag.Pictures[0].Data.Data;
                using var ms = new MemoryStream(data);
                return Image.FromStream(ms);
            }
        }
        catch { }

        // 2. 查找同目录下的封面图片（按常见命名优先级）
        var dir = Path.GetDirectoryName(filePath);
        if (dir != null)
        {
            var coverNames = new[] { "cover.jpg", "cover.png", "folder.jpg", "folder.png",
                                 "album.jpg", "album.png", "front.jpg", "front.png",
                                 "albumart.jpg", "albumart.png" };
            foreach (var name in coverNames)
            {
                var coverPath = Path.Combine(dir, name);
                if (File.Exists(coverPath))
                {
                    try { return Image.FromFile(coverPath); }
                    catch { }
                }
            }

            // 3. 兜底：目录下任意 jpg/png 图片
            foreach (var img in Directory.EnumerateFiles(dir)
                         .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                     f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                     f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)))
            {
                try { return Image.FromFile(img); }
                catch { }
            }
        }

        return null;
    }

    /// 播放进度回调：更新时间标签、进度条和歌词高亮。 
    private void OnProgressChanged(TimeSpan current, TimeSpan total)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnProgressChanged(current, total));
            return;
        }

        lblTime.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";

        if (total.TotalSeconds > 0)
            progressSlider.Value = current.TotalSeconds / total.TotalSeconds;

        if (_lyrics.Count > 0)
        {
            var idx = new LyricParser().FindCurrentLine(_lyrics, current);
            lyricView.UpdateLyrics(_lyrics, idx);
        }
    }

    /// 播放结束回调：自动播放下一首。 
    private void OnSongFinished()
    {
        if (InvokeRequired)
        {
            Invoke(OnSongFinished);
            return;
        }

        if (_currentIndex + 1 < _playlist.Count)
        {
            _currentIndex++;
            PlayCurrent();
        }
    }

    /// 播放/暂停切换。 
    private void btnPlay_Click(object? sender, EventArgs e)
    {
        if (_player.IsPlaying)
        {
            _player.Pause();
            btnPlay.Text = "▶";
        }
        else
        {
            _player.Resume();
            btnPlay.Text = "⏸";
        }
    }

    /// 上一首。 
    private void btnPrev_Click(object? sender, EventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            PlayCurrent();
        }
    }

    /// 下一首。 
    private void btnNext_Click(object? sender, EventArgs e)
    {
        if (_currentIndex + 1 < _playlist.Count)
        {
            _currentIndex++;
            PlayCurrent();
        }
    }

    /// 进度条拖拽跳转。 
    private void progressSlider_Seeked(double percent)
    {
        _player.Seek(percent);
    }

    /// 释放播放器资源。 
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _player.Dispose();
        base.OnFormClosing(e);
    }

    // ============================================================
    // 自定义圆形滑块进度条
    // ============================================================
    internal class ProgressSlider : Control
{
    private double _value;
    private bool _isDragging;

    private readonly Color _accentColor = Color.FromArgb(29, 185, 84);
    private readonly Color _trackColor = Color.FromArgb(60, 60, 60);
    private readonly Color _knobBorderColor = Color.White;

    private const int BarHeight = 4;
    private const int KnobRadius = 7;
    private const int PaddingX = 12;

    public double Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, 0, 1);
            if (Math.Abs(_value - clamped) > 0.001)
            {
                _value = clamped;
                Invalidate();
            }
        }
    }

    public event Action<double>? Seeked;

    public ProgressSlider()
    {
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int barY = Height / 2;
        int barLeft = PaddingX;
        int barRight = Width - PaddingX;
        int barWidth = barRight - barLeft;

        // 背景轨道
        using var trackBrush = new SolidBrush(_trackColor);
        g.FillRectangle(trackBrush, barLeft, barY - BarHeight / 2, barWidth, BarHeight);

        // 已播放轨道
        int filledWidth = (int)(barWidth * _value);
        if (filledWidth > 0)
        {
            using var fillBrush = new SolidBrush(_accentColor);
            g.FillRectangle(fillBrush, barLeft, barY - BarHeight / 2, filledWidth, BarHeight);
        }

        // 圆形滑块
        int knobX = barLeft + filledWidth;
        knobX = Math.Clamp(knobX, barLeft + KnobRadius, barRight - KnobRadius);

        // 滑块阴影
        using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
        {
            g.FillEllipse(shadowBrush, knobX - KnobRadius + 1, barY - KnobRadius + 2,
                KnobRadius * 2, KnobRadius * 2);
        }

        // 滑块主体
        using var knobBrush = new SolidBrush(_accentColor);
        g.FillEllipse(knobBrush, knobX - KnobRadius, barY - KnobRadius,
            KnobRadius * 2, KnobRadius * 2);

        // 滑块边框
        using var knobPen = new Pen(_knobBorderColor, 2f);
        g.DrawEllipse(knobPen, knobX - KnobRadius, barY - KnobRadius,
            KnobRadius * 2, KnobRadius * 2);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            UpdateValueFromMouse(e.X);
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isDragging)
            UpdateValueFromMouse(e.X);
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            UpdateValueFromMouse(e.X);
            Seeked?.Invoke(_value);
        }
        base.OnMouseUp(e);
    }

    private void UpdateValueFromMouse(int mouseX)
    {
        int barLeft = PaddingX;
        int barRight = Width - PaddingX;
        Value = Math.Clamp((double)(mouseX - barLeft) / (barRight - barLeft), 0, 1);
    }
}

// ============================================================
// 多行歌词视图
// ============================================================
internal class LyricView : Control
{
    private List<LyricLine> _lyrics = new();
    private int _currentIndex = -1;

    private readonly Color _accentColor = Color.FromArgb(29, 185, 84);
    private readonly Color _currentTextColor = Color.FromArgb(29, 185, 84);
    private readonly Color _nearTextColor = Color.FromArgb(160, 160, 160);
    private readonly Color _farTextColor = Color.FromArgb(100, 100, 100);
    private readonly Color _emptyTextColor = Color.FromArgb(120, 120, 120);

    public LyricView()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw, true);
    }

    public void Reset()
    {
        _lyrics = new List<LyricLine>();
        _currentIndex = -1;
        Invalidate();
    }

    public void UpdateLyrics(List<LyricLine> lyrics, int currentIndex)
    {
        if (_lyrics != lyrics || _currentIndex != currentIndex)
        {
            _lyrics = lyrics;
            _currentIndex = currentIndex;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // 背景
        using (var bgBrush = new SolidBrush(BackColor))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        if (_lyrics.Count == 0)
        {
            DrawEmptyState(g);
            return;
        }

        int visibleLines = 5; // 当前行 + 前后各2行
        int lineSpacing = 36;
        int centerY = Height / 2;

        for (int offset = -2; offset <= 2; offset++)
        {
            int idx = _currentIndex + offset;
            if (idx < 0 || idx >= _lyrics.Count) continue;

            bool isCurrent = offset == 0;
            float fontSize = isCurrent ? 13f : (Math.Abs(offset) == 1 ? 10f : 8f);
            var fontStyle = isCurrent ? FontStyle.Bold : FontStyle.Regular;

            Color textColor;
            if (isCurrent)
                textColor = _currentTextColor;
            else if (Math.Abs(offset) == 1)
                textColor = _nearTextColor;
            else
                textColor = _farTextColor;

            using var font = new Font("微软雅黑", fontSize, fontStyle);
            using var brush = new SolidBrush(textColor);

            var text = _lyrics[idx].Text;
            var size = g.MeasureString(text, font);
            float x = (Width - size.Width) / 2;
            float y = centerY + offset * lineSpacing - size.Height / 2;

            // 确保文字不超出边界
            if (y + size.Height > 0 && y < Height)
            {
                g.DrawString(text, font, brush, Math.Max(0, x), y);
            }
        }

        // 如果当前行不是第一行，在顶部画一个渐变遮罩
        // 省略，保持简洁
    }

    private void DrawEmptyState(Graphics g)
    {
        var text = "暂无歌词";
        using var font = new Font("微软雅黑", 11, FontStyle.Regular);
        using var brush = new SolidBrush(_emptyTextColor);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush,
            (Width - size.Width) / 2, (Height - size.Height) / 2);
    }
}

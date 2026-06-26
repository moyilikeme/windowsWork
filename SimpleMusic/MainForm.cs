using SimpleMusic.Models;

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
    private TrackBar trackBar;
    private ListBox listBoxSongs;
    private Label lblLyric;
    private Button btnScan;

    public MainForm()
    {
        InitializeComponent();
        _player.ProgressChanged += OnProgressChanged;
        _player.PlaybackFinished += OnSongFinished;
    }

    private void InitializeComponent()
    {
        // ========== 窗体设置 ==========
        this.Text = "SimpleMusic";
        this.Size = new Size(500, 650);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = _bgColor;
        this.FormBorderStyle = FormBorderStyle.None;  // 无边框

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

        // 标题文字
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

        // ========== 专辑封面 ==========
        picCover = new PictureBox
        {
            Location = new Point(150, 50),
            Size = new Size(200, 200),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(50, 50, 50)
        };
        this.Controls.Add(picCover);

        // ========== 歌曲信息 ==========
        lblTitle = new Label
        {
            Location = new Point(50, 265),
            Size = new Size(400, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = _textColor,
            Text = "未播放"
        };
        this.Controls.Add(lblTitle);

        lblArtist = new Label
        {
            Location = new Point(50, 298),
            Size = new Size(400, 22),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("微软雅黑", 10),
            ForeColor = _subTextColor,
            Text = "选择歌曲开始播放"
        };
        this.Controls.Add(lblArtist);

        lblTime = new Label
        {
            Location = new Point(50, 328),
            Size = new Size(400, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 9),
            ForeColor = _subTextColor,
            Text = "00:00 / 00:00"
        };
        this.Controls.Add(lblTime);

        // ========== 播放控制按钮 ==========
        btnPrev = CreateCircleButton("◀◀", 140);
        btnPlay = CreateCircleButton("▶", 200, true);  // 大一点的播放按钮
        btnNext = CreateCircleButton("▶▶", 260);

        this.Controls.Add(btnPrev);
        this.Controls.Add(btnPlay);
        this.Controls.Add(btnNext);

        // ========== 进度条 ==========
        trackBar = new TrackBar
        {
            Location = new Point(50, 390),
            Size = new Size(400, 30),
            Maximum = 100,
            TickFrequency = 10,
            BackColor = _bgColor
        };
        // 自定义进度条颜色（需要额外处理，见下方）
        this.Controls.Add(trackBar);

        // ========== 播放列表 ==========
        var lblPlaylist = new Label
        {
            Text = "  播放列表",
            Location = new Point(50, 430),
            Size = new Size(100, 25),
            ForeColor = _textColor,
            Font = new Font("微软雅黑", 10, FontStyle.Bold)
        };
        this.Controls.Add(lblPlaylist);

        listBoxSongs = new ListBox
        {
            Location = new Point(50, 458),
            Size = new Size(400, 120),
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = _textColor,
            BorderStyle = BorderStyle.None,
            Font = new Font("微软雅黑", 9),
            ItemHeight = 28
        };
        // 自定义绘制列表项
        listBoxSongs.DrawMode = DrawMode.OwnerDrawFixed;
        listBoxSongs.DrawItem += ListBoxSongs_DrawItem;
        this.Controls.Add(listBoxSongs);

        // ========== 扫描按钮 ==========
        btnScan = new Button
        {
            Text = "扫描文件夹",
            Location = new Point(190, 590),
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

        // ========== 歌词显示 ==========
        lblLyric = new Label
        {
            Location = new Point(50, 630),
            Size = new Size(400, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = _accentColor,
            Font = new Font("微软雅黑", 11, FontStyle.Regular),
            Text = ""
        };
        this.Controls.Add(lblLyric);

        // ========== 事件绑定 ==========
        btnScan.Click += btnScan_Click;
        listBoxSongs.DoubleClick += listBoxSongs_DoubleClick;
        btnPlay.Click += btnPlay_Click;
        btnPrev.Click += btnPrev_Click;
        btnNext.Click += btnNext_Click;
        trackBar.MouseUp += trackBar_MouseUp;
    }

    // 创建圆形按钮
    private Button CreateCircleButton(string text, int x, bool isLarge = false)
    {
        var size = isLarge ? 50 : 40;
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, 340),
            Size = new Size(size, size),
            FlatStyle = FlatStyle.Flat,
            ForeColor = _textColor,
            BackColor = isLarge ? _accentColor : _hoverColor,
            Font = new Font("微软雅黑", isLarge ? 12 : 9),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;

        // 鼠标悬停效果
        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = isLarge ? Color.FromArgb(50, 205, 100) : Color.FromArgb(60, 60, 60);
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = isLarge ? _accentColor : _hoverColor;
        };

        // 画圆角（通过Region实现）
        var path = new System.Drawing.Drawing2D.GraphicsPath();
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

        // 背景
        var backColor = isSelected ? _accentColor : (e.Index % 2 == 0 ? Color.FromArgb(40, 40, 40) : Color.FromArgb(45, 45, 45));
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

    // ========== 原有逻辑不变 ==========
    private void btnScan_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            btnScan.Enabled = false;
            _ = LoadSongsAsync(dlg.SelectedPath);
        }
    }

    private async Task LoadSongsAsync(string folder)
    {
        _playlist = await new MusicScanner().ScanFolderAsync(folder);

        using var db = new MusicDb("localhost", "simplemusic", "root", "123456");
        db.SaveSongs(_playlist);

        if (InvokeRequired)
        {
            Invoke(() => UpdateList());
            return;
        }
        UpdateList();
        btnScan.Enabled = true;
    }

    private void UpdateList()
    {
        listBoxSongs.DataSource = null;
        listBoxSongs.DataSource = _playlist;
    }

    private void listBoxSongs_DoubleClick(object? sender, EventArgs e)
    {
        _currentIndex = listBoxSongs.SelectedIndex;
        PlayCurrent();
    }

    private void PlayCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= _playlist.Count) return;

        var song = _playlist[_currentIndex];
        _player.Play(song.FilePath);

        lblTitle.Text = song.Title;
        lblArtist.Text = song.Artist;
        picCover.Image = LoadCover(song.FilePath);

        var lrcPath = Path.ChangeExtension(song.FilePath, ".lrc");
        _lyrics = File.Exists(lrcPath)
            ? new LyricParser().Parse(File.ReadAllText(lrcPath))
            : new List<LyricLine>();
    }

    private Image? LoadCover(string filePath)
    {
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
        return null;
    }

    private void OnProgressChanged(TimeSpan current, TimeSpan total)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnProgressChanged(current, total));
            return;
        }

        lblTime.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
        if (total.TotalSeconds > 0)
            trackBar.Value = (int)(current.TotalSeconds / total.TotalSeconds * 100);

        if (_lyrics.Count > 0)
        {
            var idx = new LyricParser().FindCurrentLine(_lyrics, current);
            lblLyric.Text = idx >= 0 ? _lyrics[idx].Text : "";
        }
    }

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

    private void btnPrev_Click(object? sender, EventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            PlayCurrent();
        }
    }

    private void btnNext_Click(object? sender, EventArgs e)
    {
        if (_currentIndex + 1 < _playlist.Count)
        {
            _currentIndex++;
            PlayCurrent();
        }
    }

    private void trackBar_MouseUp(object? sender, MouseEventArgs e)
    {
        _player.Seek(trackBar.Value / 100.0);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _player.Dispose();
        base.OnFormClosing(e);
    }
}
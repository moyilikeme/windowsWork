using SimpleMusic.Models;
using System.IO;

namespace SimpleMusic;

/// 音频文件扫描器，递归遍历文件夹提取音频元数据。
public class MusicScanner
{
    // 支持的音频文件扩展名
    private readonly string[] _extensions = { ".mp3", ".flac", ".wav" };

    public async Task<List<Song>> ScanFolderAsync(string folder)
    {
        var songs = new List<Song>();

        // 在后台线程执行 IO 密集型操作，避免阻塞 UI
        await Task.Run(() =>
        {
            // 递归遍历所有文件，比 GetFiles 更省内存（惰性返回）
            foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                // 过滤出支持的音频格式（忽略大小写）
                if (_extensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    try
                    {
                        // 使用 TagLib 读取音频文件的 ID3/FLAC 等元数据标签
                        var tag = TagLib.File.Create(file);
                        songs.Add(new Song
                        {
                            FilePath = file,
                            // 标题为空时回退到文件名
                            Title = tag.Tag.Title ?? Path.GetFileNameWithoutExtension(file),
                            // 歌手为空时显示"未知歌手"
                            Artist = tag.Tag.FirstPerformer ?? "未知歌手",
                            Duration = tag.Properties.Duration
                        });
                    }
                    catch { /* 跳过损坏或无权限的文件，继续扫描 */ }
                }
            }
        });

        return songs;
    }
}
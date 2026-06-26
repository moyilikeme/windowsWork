using SimpleMusic.Models;
using System.IO;

namespace SimpleMusic;

public class MusicScanner
{
	private readonly string[] _extensions = { ".mp3", ".flac", ".wav" };

	public async Task<List<Song>> ScanFolderAsync(string folder)
	{
		var songs = new List<Song>();

		await Task.Run(() =>
		{
			foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
			{
				if (_extensions.Contains(Path.GetExtension(file).ToLower()))
				{
					try
					{
						var tag = TagLib.File.Create(file);
						songs.Add(new Song
						{
							FilePath = file,
							Title = tag.Tag.Title ?? Path.GetFileNameWithoutExtension(file),
							Artist = tag.Tag.FirstPerformer ?? "未知歌手",
							Duration = tag.Properties.Duration
						});
					}
					catch { /* 跳过损坏文件 */ }
				}
			}
		});

		return songs;
	}
}
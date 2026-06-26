using Dapper;
using MySql.Data.MySqlClient;
using SimpleMusic.Models;

namespace SimpleMusic;

public class MusicDb : IDisposable
{
    private readonly MySqlConnection _conn;

    public MusicDb(string server, string database, string user, string password)
    {
        var connStr = $"Server={server};Database={database};User ID={user};Password={password};Charset=utf8mb4;";
        _conn = new MySqlConnection(connStr);
        _conn.Open();

        _conn.Execute(@"
            CREATE TABLE IF NOT EXISTS Songs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Path VARCHAR(500) UNIQUE,
                Title VARCHAR(200),
                Artist VARCHAR(200),
                Duration INT
            )");
    }

    public void SaveSongs(List<Song> songs)
    {
        using var tx = _conn.BeginTransaction();
        foreach (var s in songs)
        {
            _conn.Execute(@"
                INSERT INTO Songs (Path, Title, Artist, Duration)
                VALUES (@FilePath, @Title, @Artist, @Duration)
                ON DUPLICATE KEY UPDATE 
                    Title = @Title, 
                    Artist = @Artist, 
                    Duration = @Duration",
                new { s.FilePath, s.Title, s.Artist, Duration = (int)s.Duration.TotalSeconds }, tx);
        }
        tx.Commit();
    }

    public List<Song> GetAllSongs() =>
        _conn.Query<Song>("SELECT FilePath, Title, Artist, Duration FROM Songs").ToList();

    public void Dispose() => _conn.Dispose();
}
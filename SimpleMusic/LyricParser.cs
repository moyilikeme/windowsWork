using System.Text.RegularExpressions;

namespace SimpleMusic;

public class LyricLine
{
    public TimeSpan Time { get; set; }
    public string Text { get; set; } = "";
}

public class LyricParser
{
    public List<LyricLine> Parse(string lrcText)
    {
        var lines = new List<LyricLine>();
        var regex = new Regex(@"\[(\d{2}):(\d{2})\.(\d{2,3})\](.+)");

        foreach (var line in lrcText.Split('\n'))
        {
            var match = regex.Match(line.Trim());
            if (match.Success)
            {
                var min = int.Parse(match.Groups[1].Value);
                var sec = int.Parse(match.Groups[2].Value);
                var msStr = match.Groups[3].Value;
                var ms = msStr.Length == 2 ? int.Parse(msStr) * 10 : int.Parse(msStr);

                lines.Add(new LyricLine
                {
                    Time = TimeSpan.FromMilliseconds(min * 60000 + sec * 1000 + ms),
                    Text = match.Groups[4].Value
                });
            }
        }
        return lines.OrderBy(l => l.Time).ToList();
    }

    public int FindCurrentLine(List<LyricLine> lines, TimeSpan current)
    {
        for (int i = lines.Count - 1; i >= 0; i--)
            if (lines[i].Time <= current) return i;
        return -1;
    }
}
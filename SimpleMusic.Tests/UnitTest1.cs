using SimpleMusic;

namespace SimpleMusic.Tests;

public class LyricParserTests
{
    [Fact]
    public void Parse_ValidLrc_ReturnsOrderedLines()
    {
        var lrc = "[00:01.50]第一行\n[00:03.00]第二行";
        var result = new LyricParser().Parse(lrc);

        Assert.Equal(2, result.Count);
        Assert.Equal("第一行", result[0].Text);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), result[0].Time);
    }

    [Fact]
    public void FindCurrentLine_MiddleTime_ReturnsCorrectIndex()
    {
        var lines = new List<LyricLine>
        {
            new LyricLine { Time = TimeSpan.FromSeconds(1), Text = "A" },
            new LyricLine { Time = TimeSpan.FromSeconds(3), Text = "B" },
            new LyricLine { Time = TimeSpan.FromSeconds(5), Text = "C" }
        };

        var parser = new LyricParser();
        Assert.Equal(0, parser.FindCurrentLine(lines, TimeSpan.FromSeconds(2)));
        Assert.Equal(1, parser.FindCurrentLine(lines, TimeSpan.FromSeconds(4)));
    }
}
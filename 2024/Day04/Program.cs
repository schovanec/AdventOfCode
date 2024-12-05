using System.Collections.Immutable;

var board = Board.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var matchCount = board.Content.Keys.Sum(p => board.CountMatchesAt(p, "XMAS"));
Console.WriteLine($"Part 1 Result = {matchCount}");

var patternCount = board.Content.Keys.Count(board.MatchPatternAt);
Console.WriteLine($"Part 2 Result = {patternCount}");

record struct Point(int X, int Y)
{
  public Point Offset(int dx, int dy) => new(X + dx, Y + dy);
}

record Board(ImmutableDictionary<Point, char> Content)
{
  public char this[Point pt] => Content.GetValueOrDefault(pt);

  public int CountMatchesAt(Point pt, ReadOnlySpan<char> word)
  {
    if (this[pt] != word[0])
      return 0;

    var result = 0;
    for (var dx = -1; dx <= 1; ++dx)
    {
      for (var dy = -1; dy <= 1; ++dy)
      {
        if (dy != 0 || dx != 0)
        {
          if (MatchAlong(pt, new(dx, dy), word))
            ++result;
        }
      }
    }

    return result;
  }

  public bool MatchPatternAt(Point pt)
    => this[pt] == 'A'
    && (MatchAlong(pt.Offset(-1, -1), new(1, 1), "MAS") || MatchAlong(pt.Offset(1, 1), new(-1, -1), "MAS"))
    && (MatchAlong(pt.Offset(1, -1), new(-1, 1), "MAS") || MatchAlong(pt.Offset(-1, 1), new(1, -1), "MAS"));

  private bool MatchAlong(Point pt, Point dir, ReadOnlySpan<char> word)
  {
    for (var i = 0; i < word.Length; ++i)
    {
      if (Content.GetValueOrDefault(new(pt.X + dir.X * i, pt.Y + dir.Y * i)) != word[i])
        return false;
    }

    return true;
  }

  public static Board Parse(IEnumerable<string> input)
  {
    var result = ImmutableDictionary.CreateBuilder<Point, char>();

    int y = 0;
    foreach (var line in input)
    {
      for (var x = 0; x < line.Length; ++x)
        result[new(x, y)] = line[x];

      ++y;
    }

    return new(result.ToImmutable());
  }
}
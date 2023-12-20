var steps = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(PlanStep.Parse)
                .ToArray();

var lines1 = ExecutePlan(steps).ToList();
var result1 = FindArea(lines1);
Console.WriteLine($"Part 1 Result = {result1}");

var realSteps = steps.Select(x => x.ExtractRealStep()).ToArray();
var lines2 = ExecutePlan(realSteps).ToList();
var result2 = FindArea(lines2);
Console.WriteLine($"Part 2 Result = {result2}");

IEnumerable<Line> ExecutePlan(IEnumerable<PlanStep> steps)
{
  Vector pos = new(0, 0);

  foreach (var step in steps)
  {
    var line = step.ToLine(pos);
    yield return line;
    pos = line.End;
  }
}

long FindArea(IList<Line> lines)
{
  long totalArea = 0L;

  var horiz = lines.Where(x => x.IsHorizontal);
  var vert = lines.Where(x => !x.IsHorizontal);

  var vertMap = (from ln in vert
                 from pt in ln.EnumEnds()
                 select (pt, ln)).ToDictionary(x => x.pt, x => x.ln);

  long? prevY = default;
  foreach (var g in horiz.GroupBy(x => x.Start.Y).OrderBy(g => g.Key))
  {
    var curY = g.Key;

    var ranges = g.Select(ln => (min: ln.MinX, max: ln.MaxX)).ToList();

    var other = vert.Where(ln => ln.MinY <= curY && ln.MaxY >= curY)
                    .Select(ln => ln.Start.X)
                    .Where(x => !ranges.Any(r => r.min <= x && r.max >= x))
                    .Select(x => (min: x, max: x));

    ranges = [.. ranges, .. other];
    ranges.Sort((r1, r2) => r1.min.CompareTo(r2.min));

    long? startX = default;
    foreach (var r in ranges)
    {
      totalArea += r.max - r.min + 1;

      if (startX.HasValue)
      {
        totalArea += r.min - startX.Value - 1;
        startX = default;

        if (vertMap.TryGetValue(new(r.min, curY), out var ln1) &&
            vertMap.TryGetValue(new(r.max, curY), out var ln2))
        {
          if ((ln1.MinY == curY && ln2.MinY == curY) ||
              (ln1.MaxY == curY && ln2.MaxY == curY))
          {
            startX = r.max;
          }
        }

      }
      else
      {
        if (r.min == r.max)
        {
          startX = r.max;
        }
        else if (vertMap.TryGetValue(new(r.min, curY), out var ln1) &&
                 vertMap.TryGetValue(new(r.max, curY), out var ln2))
        {
          if ((ln1.MinY == curY && ln2.MaxY == curY) ||
              (ln1.MaxY == curY && ln2.MinY == curY))
          {
            startX = r.max;
          }
        }
      }
    }

    if (prevY.HasValue)
    {
      var h = curY - prevY.Value - 1;
      if (h > 0)
      {
        var vx = (from ln in vert
                  where ln.MinY < curY && ln.MaxY > prevY.Value
                  orderby ln.Start.X
                  select ln.Start.X).ToArray();

        for (var i = 0; i + 1 < vx.Length; i += 2)
        {
          var w = vx[i + 1] - vx[i] + 1;
          totalArea += w * h;
        }
      }
    }

    prevY = g.Key;
  }

  return totalArea;
}

record struct Vector(long X, long Y)
{
  public Vector Add(Vector other) => new(X + other.X, Y + other.Y);

  public Vector Scale(long scale) => new(X * scale, Y * scale);

  public IEnumerable<Vector> EnumAdjacent()
    => [new(X - 1, Y), new(X + 1, Y), new(X, Y - 1), new(X, Y + 1)];
}

record struct Line(Vector Start, Vector End)
{
  public bool IsHorizontal => Start.Y == End.Y;

  public long MinX => Math.Min(Start.X, End.X);

  public long MinY => Math.Min(Start.Y, End.Y);

  public long MaxX => Math.Max(Start.X, End.X);

  public long MaxY => Math.Max(Start.Y, End.Y);

  public long GetLength()
    => IsHorizontal
      ? Math.Abs(End.X - Start.X) + 1
      : Math.Abs(End.Y - Start.Y) + 1;

  public IEnumerable<Vector> EnumEnds() => [Start, End];
}

record PlanStep(char Direction, long Distance, string Color)
{
  public Vector GetDirectionVector()
    => Direction switch
    {
      'U' => new(0, -1),
      'D' => new(0, 1),
      'L' => new(-1, 0),
      'R' => new(1, 0),
      _ => default
    };

  public Line ToLine(Vector start)
    => new(start, GetDirectionVector().Scale(Distance).Add(start));

  public PlanStep ExtractRealStep()
    => new(
      Color[5] switch
      {
        '0' => 'R',
        '1' => 'D',
        '2' => 'L',
        '3' => 'U',
        _ => default
      },
      long.Parse(Color[..5], System.Globalization.NumberStyles.HexNumber),
      string.Empty
    );

  public static PlanStep Parse(string input)
  {
    var parts = input.Split(' ', 3);
    return new(
      parts[0][0],
      long.Parse(parts[1]),
      parts[2].Trim('(', ')', '#'));
  }
}
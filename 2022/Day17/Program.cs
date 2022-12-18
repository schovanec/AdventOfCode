var jets = File.ReadLines(args.FirstOrDefault() ?? "input.txt").First();

var result1 = Stack(jets).Take(2022).ToList();
Console.WriteLine($"Part 1 Result = {result1.Sum()}");

var result2 = StackHuge(jets, 1_000_000_000_000);
Console.WriteLine($"Part 2 Result = {result2}");

static long StackHuge(string jets, long size)
{
  var (prefix, repeat) = FindPattern(jets);

  if (size < prefix.Length)
    return prefix.Take((int)size).Sum();

  long result = prefix.Sum();

  size -= prefix.Length;
  result += (size / repeat.Length) * repeat.Sum();

  var remainder = size % repeat.Length;
  result += repeat.Take((int)remainder).Sum();

  return result;
}

static (int[] prefix, int[] repeat) FindPattern(string jets)
{
  var sample = Stack(jets).Take(jets.Length * 2).ToArray();

  for (var i = 0; i <= jets.Length; i += 5)
  {
    if (i == 0)
      continue;

    var prefixLength = sample.Zip(sample.Skip(i))
                             .Select((x, i) => (x.First, x.Second, Index: i))
                             .Where(x => x.First != x.Second)
                             .Select(x => x.Index)
                             .DefaultIfEmpty(-1)
                             .Last();

    if (prefixLength >= 0 && prefixLength < jets.Length)
    {
      var start = prefixLength + 1;
      var end = start + i;
      return (sample[..start], sample[start..end]);
    }
  }

  return default;
}

static IEnumerable<int> Stack(string jets)
{
  HashSet<(int x, int y)> stopped = new();
  var highWaterMark = 0;
  var jetIndex = 0;

  int i = 0;
  while (true)
  {
    var current = Rock.Shapes[i].MoveTo(2, highWaterMark + 4);
    var falling = true;
    while (falling)
    {
      var shifted = current.Shift(jets[jetIndex]);
      jetIndex = (jetIndex + 1) % jets.Length;

      if (shifted.MinX >= 0 && shifted.MaxX < 7 && !shifted.AllPoints.Any(p => stopped.Contains(p)))
        current = shifted;

      var dropped = current.ShiftDown();
      if (dropped.MinY > 0 && !dropped.AllPoints.Any(p => stopped.Contains(p)))
        current = dropped;
      else
        falling = false;
    }

    foreach (var p in current.AllPoints)
      stopped.Add(p);

    var oldHighWaterMark = highWaterMark;
    highWaterMark = Math.Max(highWaterMark, current.MaxY);

    yield return highWaterMark - oldHighWaterMark;

    i++;
    i %= Rock.ShapeCount;
  }
}

record Rock((int x, int y)[] Image, (int x, int y) Offset = default)
{
  public int Width { get; } = Image.Max(p => p.x + 1);

  public int Height { get; } = Image.Max(p => p.y + 1);

  public int MinX => Offset.x;

  public int MinY => Offset.y;

  public int MaxX => Offset.x + Width - 1;

  public int MaxY => Offset.y + Height - 1;

  public IEnumerable<(int x, int y)> AllPoints
    => Image.Select(p => (p.x + Offset.x, p.y + Offset.y));

  public Rock MoveTo(int x, int y)
    => this with { Offset = (x, y) };

  public Rock Shift(char direction)
    => direction switch
    {
      '<' => this with { Offset = (Offset.x - 1, Offset.y) },
      '>' => this with { Offset = (Offset.x + 1, Offset.y) },
      _ => this
    };

  public Rock ShiftDown()
    => this with { Offset = (Offset.x, Offset.y - 1) };

  public static readonly Rock[] Shapes = new Rock[]
  {
    new (new [] { (0, 0), (1, 0), (2, 0), (3, 0) }),
    new (new [] { (1, 0), (0, 1), (1, 1), (2, 1), (1, 2) }),
    new (new [] { (0, 0), (1, 0), (2, 0), (2, 1), (2, 2) }),
    new (new [] { (0, 0), (0, 1), (0, 2), (0, 3) }),
    new (new [] { (0, 0), (1, 0), (0, 1), (1, 1) }),
  };

  public static int ShapeCount => Shapes.Length;
}
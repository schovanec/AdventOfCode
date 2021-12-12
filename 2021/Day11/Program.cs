using System.Collections.Immutable;

var input = Board.Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var totalFirst100 = Simulate(input).Take(100).Sum(x => x.flashCount);
Console.WriteLine($"Part 1 Result = {totalFirst100}");

var octopusCount = input.EnergyLevels.Length;
var (firstAllFlashStep, _) = Simulate(input).SkipWhile(x => x.flashCount != octopusCount).First();
Console.WriteLine($"Part 2 Result = {firstAllFlashStep}");

IEnumerable<(int step, int flashCount)> Simulate(Board board)
{
  var step = 0;
  while (true)
  {
    (board, var count) = Step(board);
    yield return (++step, count);
  }
}

(Board board, int flashCount) Step(Board board)
{
  var builder = board.EnergyLevels.ToBuilder();

  for (var i = 0; i < builder.Count; ++i)
    builder[i]++;

  var seen = new HashSet<int>();
  while (true)
  {
    var flashing = Enumerable.Range(0, builder.Count)
                             .Where(i => !seen.Contains(i) && builder[i] > 9)
                             .ToArray();

    if (flashing.Length == 0)
      break;

    seen.UnionWith(flashing);

    foreach (var adj in flashing.SelectMany(i => EnumAdjacentIndexes(i, board.Width, board.Height)))
      builder[adj]++;
  }

  foreach (var i in seen)
    builder[i] = 0;

  return (board with { EnergyLevels = builder.ToImmutable() }, seen.Count);
}

IEnumerable<int> EnumAdjacentIndexes(int index, int width, int height)
  => from i in Enumerable.Range(-1, 3)
     from j in Enumerable.Range(-1, 3)
     where i != 0 || j != 0
     let pt = (x: (index % width) + i, y: (index / width) + j)
     where pt.x >= 0 && pt.x < width && pt.y >= 0 && pt.y < height
     select (pt.y * width) + pt.x;

record Board(ImmutableArray<int> EnergyLevels, int Width)
{
  public int Height { get; } = EnergyLevels.Length / Width;

  public static Board Parse(string[] input)
    => new Board(
      input.SelectMany(x => x.Select(ch => ch - '0')).ToImmutableArray(),
      input.First().Length);
}
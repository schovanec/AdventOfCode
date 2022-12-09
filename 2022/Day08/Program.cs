var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(line => line.Select(ch => ch - '0').ToArray())
                .ToArray();

DoPart1(input);
DoPart2(input);

static void DoPart1(int[][] map)
{
  var size = map.Length;
  var visible = (from d in EnumDirections()
                 from i in Enumerable.Range(0, size)
                 let start = GetStartPosition(d, i, size)
                 from pos in FindVisible(map, start, d, size)
                 select pos).Distinct();

  Console.WriteLine($"Part 1 Result = {visible.Count()}");
}

static void DoPart2(int[][] map)
{
  var size = map.Length;
  var viewDistances = from x in Enumerable.Range(0, size)
                      from y in Enumerable.Range(0, size)
                      select GetScenicScore(map, (x, y), size);

  Console.WriteLine($"Part 2 Result = {viewDistances.Max()}");
}

static IEnumerable<(int x, int y)> FindVisible(int[][] map, (int x, int y) start, (int x, int y) dir, int size)
{
  var highest = -1;
  foreach (var pos in EnumPositions(start, dir, size))
  {
    var height = map[pos.y][pos.x];
    if (height > highest)
    {
      yield return pos;
      highest = height;
    }
  }
}

static int GetScenicScore(int[][] map, (int x, int y) start, int size)
  => EnumDirections().Select(d => GetViewDistance(map, start, d, size))
                     .Aggregate(1, (a, n) => a * n);

static IEnumerable<(int dx, int dy)> EnumDirections()
{
  yield return (0, -1);
  yield return (-1, 0);
  yield return (0, 1);
  yield return (1, 0);
}

static IEnumerable<(int x, int y)> EnumPositions((int x, int y) start, (int x, int y) dir, int size)
  => from i in Enumerable.Range(0, size)
     let x = start.x + (i * dir.x)
     let y = start.y + (i * dir.y)
     where x >= 0 && x < size && y >= 0 && y < size
     select (x, y);

static (int x, int y) GetStartPosition((int x, int y) dir, int pos, int size)
  => (GetStartCoordinate(dir.x, pos, size), GetStartCoordinate(dir.y, pos, size));

static int GetStartCoordinate(int delta, int pos, int size)
  => delta switch { < 0 => size - 1, 0 => pos, > 0 => 0 };

static int GetViewDistance(int[][] map, (int x, int y) start, (int x, int y) dir, int size)
{
  var height = map[start.y][start.x];
  var count = 0;
  foreach (var (x, y) in EnumPositions(start, dir, size).Skip(1))
  {
    ++count;
    if (map[y][x] >= height)
      break;
  }

  return count;
}
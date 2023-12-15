
using System.Collections.Immutable;

var input = ParseRocks(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = CalculateLoad(RollNorth(input));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = CalculateLoad(SpinMany(input, 1000000000));
Console.WriteLine($"Part 2 Result = {result2}");

ImmutableDictionary<Point, Shape> SpinMany(ImmutableDictionary<Point, Shape> map, long count)
{
  var (history, offset) = FindCycle(map);
  var period = history.Count - offset;
  var index = ((count - offset) % period) + offset;
  return history[(int)index];
}

(List<ImmutableDictionary<Point, Shape>>, int offset) FindCycle(ImmutableDictionary<Point, Shape> map)
{
  List<ImmutableDictionary<Point, Shape>> history = [map];
  while (true)
  {
    var step = Spin(history.Last());
    var pos = history.TakeWhile(x => !IsEqual(x, step)).Count();
    if (pos < history.Count)
      return (history, pos);

    history.Add(step);
  }
}

bool IsEqual(ImmutableDictionary<Point, Shape> a, ImmutableDictionary<Point, Shape> b)
  => a.Count == b.Count
  && a.All(x => b.TryGetValue(x.Key, out var v) && x.Value == v);

(int axis, int dir)[] cycleRolls = [(1, 1), (0, -1), (1, -1), (0, 1)];

ImmutableDictionary<Point, Shape> Spin(ImmutableDictionary<Point, Shape> map)
  => EnumCycleRolls().Aggregate(map, (m, r) => Roll(m, r.axis, r.dir));

static IEnumerable<(int axis, int dir)> EnumCycleRolls()
{
  yield return (1, 1);
  yield return (0, -1);
  yield return (1, -1);
  yield return (0, 1);
}

static int CalculateLoad(ImmutableDictionary<Point, Shape> map)
  => map.Where(x => x.Value == Shape.Round).Sum(x => x.Key.Y);

static ImmutableDictionary<Point, Shape> RollNorth(ImmutableDictionary<Point, Shape> map)
  => Roll(map, 1, 1);

static ImmutableDictionary<Point, Shape> Roll(ImmutableDictionary<Point, Shape> map, int axis, int direction)
{
  var limit = direction < 0
    ? map.Keys.Min(pt => pt[axis])
    : map.Keys.Max(pt => pt[axis]);

  var offset = -Math.Sign(direction);
  var axisGroup = axis == 0 ? 1 : 0;
  var result = ImmutableDictionary.CreateBuilder<Point, Shape>();
  foreach (var g in map.Keys.GroupBy(p => p[axisGroup]))
  {
    var openPos = limit;
    foreach (var pt in g.OrderBy(p => p[axis] * offset))
    {
      var shape = map[pt];
      if (shape == Shape.Cube)
      {
        result.Add(pt, shape);
        openPos = pt[axis] + offset;
      }
      else
      {
        result.Add(pt.Set(axis, openPos), shape);
        openPos += offset;
      }
    }
  }

  return result.ToImmutable();
}

static ImmutableDictionary<Point, Shape> ParseRocks(string[] input)
  => (from i in Enumerable.Range(0, input.Length)
      let line = input[i]
      let y = input.Length - i
      from x in Enumerable.Range(0, line.Length)
      let shape = line[x] switch { 'O' => Shape.Round, '#' => Shape.Cube, _ => default(Shape?) }
      where shape.HasValue
      select (p: new Point(x, y), shape: shape.Value)).ToImmutableDictionary(x => x.p, x => x.shape);

enum Shape { Round, Cube };

record struct Point(int X, int Y)
{
  public int this[int axis] => axis switch { 0 => X, 1 => Y, _ => throw new ArgumentException(nameof(axis)) };

  public Point Set(int axis, int value)
    => axis switch
    {
      0 => new(value, Y),
      1 => new(X, value),
      _ => this
    };
}

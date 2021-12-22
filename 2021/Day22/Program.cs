using System.Collections.Immutable;
using System.Text.RegularExpressions;

var input = Parse(File.ReadLines(args.FirstOrDefault() ?? "input.txt")).ToList().AsReadOnly();

var count1 = ExecuteCommands(input.Where(cmd => cmd.IsInit));
Console.WriteLine($"Part 1 Result = {count1}");

var count2 = ExecuteCommands(input);
Console.WriteLine($"Part 1 Result = {count2}");

long ExecuteCommands(IEnumerable<Command> commands)
{
  var regionsTurnedOn = ImmutableList.Create<Cube>();

  foreach (var cmd in commands)
  {
    switch (cmd.Action)
    {
      case Action.On:
        var regionsToEnable = ImmutableList.Create(cmd.Cube);
        foreach (var existing in regionsTurnedOn)
        {
          regionsToEnable = regionsToEnable.SelectMany(a => a.Subtract(existing))
                                           .Where(a => !a.IsEmpty)
                                           .ToImmutableList();
        }

        regionsTurnedOn = regionsTurnedOn.AddRange(regionsToEnable);
        break;

      case Action.Off:
        regionsTurnedOn = regionsTurnedOn.SelectMany(a => a.Subtract(cmd.Cube))
                                         .Where(a => !a.IsEmpty)
                                         .ToImmutableList();
        break;
    }
  }

  return regionsTurnedOn.Sum(r => r.Volume);
}

IEnumerable<Command> Parse(IEnumerable<string> input)
{
  foreach (var line in input)
  {
    var m = Regex.Match(
      line,
      @"^(?<cmd>on|off) x=(?<xmin>-?\d+)\.\.(?<xmax>-?\d+),y=(?<ymin>-?\d+)\.\.(?<ymax>-?\d+),z=(?<zmin>-?\d+)\.\.(?<zmax>-?\d+)$",
      RegexOptions.Singleline);

    if (m.Success)
    {
      var action = Enum.Parse<Action>(m.Groups["cmd"].Value, true);

      var x = new Interval(
        int.Parse(m.Groups["xmin"].Value),
        int.Parse(m.Groups["xmax"].Value));

      var y = new Interval(
        int.Parse(m.Groups["ymin"].Value),
        int.Parse(m.Groups["ymax"].Value));

      var z = new Interval(
        int.Parse(m.Groups["zmin"].Value),
        int.Parse(m.Groups["zmax"].Value));

      yield return new Command(action, new Cube(x, y, z));
    }
  }
}

record struct Interval(long Begin, long End)
{
  public static readonly Interval Empty = new Interval(0, -1);

  public bool IsEmpty => Size == 0L;

  public long Size => Math.Max(0, End - Begin + 1);

  public bool Contains(Interval other)
    => other.End >= Begin && other.Begin <= End;

  public Interval Intersect(Interval other)
  {
    var newBegin = Math.Max(Begin, other.Begin);
    var newEnd = Math.Min(End, other.End);
    return newBegin <= newEnd
      ? new Interval(newBegin, newEnd)
      : Empty;
  }

  public IEnumerable<Interval> Split(Interval other)
  {
    var intersect = Intersect(other);
    if (intersect.IsEmpty)
    {
      yield return this;
    }
    else
    {
      if (intersect.Begin > Begin)
        yield return new Interval(Begin, intersect.Begin - 1);

      yield return intersect;

      if (intersect.End < End)
        yield return new Interval(intersect.End + 1, End);
    }
  }

  public IEnumerable<long> Values
  {
    get
    {
      for (var i = Begin; i <= End; ++i)
        yield return i;
    }
  }
}

record struct Cube(Interval X, Interval Y, Interval Z)
{
  public long Volume
    => X.Size * Y.Size * Z.Size;

  public bool IsEmpty => X.IsEmpty || Y.IsEmpty || Z.IsEmpty;
  public Cube Intersect(Cube other)
    => new Cube(X.Intersect(other.X), Y.Intersect(other.Y), Z.Intersect(other.Z));

  public IEnumerable<Cube> SplitX(Interval interval)
  {
    var (y, z) = (Y, Z);
    return X.Split(interval).Select(x => new Cube(x, y, z));
  }

  public IEnumerable<Cube> SplitY(Interval interval)
  {
    var (x, z) = (X, Z);
    return Y.Split(interval).Select(y => new Cube(x, y, z));
  }

  public IEnumerable<Cube> SplitZ(Interval interval)
  {
    var (x, y) = (X, Y);
    return Z.Split(interval).Select(z => new Cube(x, y, z));
  }

  public IEnumerable<Cube> Subtract(Cube other)
  {
    var intersection = Intersect(other);

    if (intersection.IsEmpty)
      return Enumerable.Repeat(this, 1);
    else if (intersection == this)
      return Enumerable.Empty<Cube>();

    return SplitX(intersection.X).SelectMany(a => a.SplitY(intersection.Y))
                                 .SelectMany(a => a.SplitZ(intersection.Z))
                                 .Where(a => a != intersection);
  }
}

record Command(Action Action, Cube Cube)
{
  private static readonly Interval InitInterval = new Interval(-50, 50);

  public bool IsInit
    => InitInterval.Contains(Cube.X) &&
       InitInterval.Contains(Cube.Y) &&
       InitInterval.Contains(Cube.Z);
}

enum Action { On, Off }
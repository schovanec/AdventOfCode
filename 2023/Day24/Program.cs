using Microsoft.Z3;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(HailStone.Parse)
                .ToArray();

Part1();
Part2();

void Part1()
{
  var testAreaMin = args.Skip(1).Select(double.Parse).FirstOrDefault(200000000000000);
  var testAreaMax = args.Skip(2).Select(double.Parse).FirstOrDefault(400000000000000);

  var intersections = FindIntersectionPoints(input.Select(x => x.ProjectOntoZ()).ToList());
  var result1 = intersections.Count(p => p.X >= testAreaMin && p.X <= testAreaMax
                                      && p.Y >= testAreaMin && p.Y <= testAreaMax);
  Console.WriteLine($"Part 1 Result = {result1}");

  static IEnumerable<Vector> FindIntersectionPoints(IList<HailStone> stones)
  {
    for (var i = 0; i < stones.Count - 1; ++i)
    {
      var a = stones[i];
      for (var j = i + 1; j < stones.Count; ++j)
      {
        var b = stones[j];
        var ta = a.IntersectWith(b);
        var tb = b.IntersectWith(a);
        if (ta.HasValue && tb.HasValue)
          yield return a.AtTime(ta.Value);
      }
    }
  }
}

void Part2()
{
  using var ctx = new Context();

  RealExpr[] PR = [
    (RealExpr)ctx.MkConst("PRx", ctx.RealSort),
    (RealExpr)ctx.MkConst("PRy", ctx.RealSort),
    (RealExpr)ctx.MkConst("PRz", ctx.RealSort)];

  RealExpr[] VR = [
    (RealExpr)ctx.MkConst("VRx", ctx.RealSort),
    (RealExpr)ctx.MkConst("VRy", ctx.RealSort),
    (RealExpr)ctx.MkConst("VRz", ctx.RealSort)];

  const int count = 3;

  var solver = ctx.MkSolver();
  var stones = input.Take(count).ToArray();
  for (var i = 0; i < stones.Length; ++i)
  {
    var (p, v) = stones[i];
    var t = (RealExpr)ctx.MkConst($"t{i}", ctx.RealSort);
    solver.Assert(
      ctx.MkEq(PR[0], p.X + t * (v.X - VR[0])),
      ctx.MkEq(PR[1], p.Y + t * (v.Y - VR[1])),
      ctx.MkEq(PR[2], p.Z + t * (v.Z - VR[2])));
  }

  if (solver.Check() != Status.SATISFIABLE)
  {
    Console.WriteLine(solver.Check());
    return;
  }

  var result2 = PR.Select(exp => (RatNum)solver.Model.Evaluate(exp))
                  .Sum(n => n.Double);

  Console.WriteLine($"Part 2 Result = {result2}");
}

record struct Vector(double X, double Y, double Z)
{
  public Vector ProjectOntoZ()
    => new(X, Y, 0);

  public readonly Vector Add(Vector other)
    => new(X + other.X, Y + other.Y, Z + other.Z);

  public readonly Vector Subtract(Vector other)
    => Add(other.Negate());

  public readonly Vector Scale(double scalar)
    => new(X * scalar, Y * scalar, Z * scalar);

  public readonly Vector Negate() => new(-X, -Y, -Z);

  public readonly double Dot(Vector other)
    => (X * other.X) + (Y * other.Y) + (Z * other.Z);

  public readonly Vector Cross(Vector other)
    => new(
      (Y * other.Z) - (Z * other.Y),
      (Z * other.X) - (X * other.Z),
      (X * other.Y) - (Y * other.X));

  public static Vector Parse(string input)
  {
    var parts = input.Split(',', 3, StringSplitOptions.TrimEntries)
                     .Select(double.Parse)
                     .ToArray();

    return new(parts[0], parts[1], parts[2]);
  }
}

record HailStone(Vector Position, Vector Velocity)
{
  public HailStone ProjectOntoZ()
    => new(Position.ProjectOntoZ(), Velocity.ProjectOntoZ());

  public double? IntersectWith(HailStone other)
  {
    var a = Velocity.Cross(other.Velocity);
    var d = a.Dot(a);

    if (d == 0)
      return default;

    var b = other.Position.Subtract(Position).Cross(other.Velocity);

    var t = b.Dot(a) / d;
    if (t < 0)
      return default;

    return t;
  }

  public Vector AtTime(double t)
    => Position.Add(Velocity.Scale(t));

  public static HailStone Parse(string input)
  {
    var parts = input.Split('@', 2, StringSplitOptions.TrimEntries)
                     .Select(Vector.Parse)
                     .ToArray();

    return new(parts[0], parts[1]);
  }
}
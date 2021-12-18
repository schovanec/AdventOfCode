var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt").Select(Box.Parse).First();

var bestMaxY = FindBestMaxY(input);
Console.WriteLine($"Part 1 Result = {bestMaxY}");

var vyOptions = FindAllVelocitiesY(input).ToLookup(v => v.t, v => v.vy);
var vxOptions = FindAllVelocitiesX(input, vyOptions.Select(x => x.Key)).ToList();

var allVelocities = vxOptions.SelectMany(x => vyOptions[x.t], (a, vy) => (a.vx, vy)).Distinct().ToList();
Console.WriteLine($"Part 2 Result = {allVelocities.Count}");

int FindBestMaxY(Box input)
{
  var best = 0;
  for (var vy = input.MinY; vy < -input.MinY; ++vy)
  {
    var yMax = (vy * (vy + 1)) / 2;
    var nMin = (long)Math.Ceiling((-1 + Math.Sqrt(1 + 8 * (yMax - input.MaxY))) / 2);
    var nMax = (long)Math.Floor((-1 + Math.Sqrt(1 + 8 * (yMax - input.MinY))) / 2);

    if (nMin <= nMax)
      best = yMax;
  }

  return best;
}

IEnumerable<(int t, int vy)> FindAllVelocitiesY(Box input)
{
  for (var vy = input.MinY; vy < -input.MinY; ++vy)
  {
    var (tMin, tMax) = ComputeTickRangeY(vy, input.MinY, input.MaxY);
    for (var t = tMin; t <= tMax; ++t)
      yield return (t, vy);
  }
}

(int tMin, int tMax) ComputeTickRangeY(int vy, int minY, int maxY)
{
  var offset = 0;
  if (vy > 0)
  {
    offset = (2 * vy) + 1;
    vy = -vy - 1;
  }
  else if (vy == 0)
  {
    offset = 1;
    vy = -1;
  }

  var b = (2 * vy) + 1;
  var tMin = (int)Math.Ceiling((-b - Math.Sqrt((b * b) - (8 * maxY))) / -2) + offset;
  var tMax = (int)Math.Floor((-b - Math.Sqrt((b * b) - (8 * minY))) / -2) + offset;

  return (tMin, tMax);
}

IEnumerable<(int t, int vx)> FindAllVelocitiesX(Box input, IEnumerable<int> times)
{
  var vxMinRest = (int)Math.Ceiling((Math.Sqrt(1 + 8 * input.MinX) - 1) / 2);
  var vxMaxRest = (int)Math.Floor((Math.Sqrt(1 + 8 * input.MaxX) - 1) / 2);

  foreach (var t in times)
  {
    if (t > vxMinRest)
    {
      for (var vx = vxMinRest; vx <= Math.Min(t, vxMaxRest); ++vx)
        yield return (t, vx);
    }
    else
    {
      var vxMin = (int)Math.Ceiling((((2 * (double)input.MinX) / t) + t - 1) / 2);
      var vxMax = (int)Math.Floor((((2 * (double)input.MaxX) / t) + t - 1) / 2);
      for (var vx = Math.Max(t, vxMin); vx <= vxMax; ++vx)
        yield return (t, vx);
    }
  }
}

record Box(int MinX, int MinY, int MaxX, int MaxY)
{
  public static Box Parse(string box)
  {
    var split = box.Split(',');
    var x = split[0].Split('=')[1].Split("..");
    var y = split[1].Split('=')[1].Split("..");

    return new Box(
      int.Parse(x[0]),
      int.Parse(y[0]),
      int.Parse(x[1]),
      int.Parse(y[1]));
  }
}
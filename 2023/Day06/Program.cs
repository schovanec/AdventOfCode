var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");

var result1 = ParseInput(input).Select(r => CountWinningOptions(r.time, r.record))
                               .Aggregate((a, b) => a * b);
Console.WriteLine($"Part 1 Result = {result1}");

var singleRace = ParseInput(input, true).Single();
var result2 = CountWinningOptions(singleRace.time, singleRace.record);
Console.WriteLine($"Part 2 Result = {result2}");

long CountWinningOptions(int raceTime, long record)
{
  var min = Enumerable.Range(1, raceTime)
                      .First(t => CalcDistance(t, raceTime) > record);

  var max = Enumerable.Range(1, raceTime)
                      .Select(t => raceTime - t)
                      .First(t => CalcDistance(t, raceTime) > record);

  return max - min + 1;
}

long CalcDistance(int pressTime, int raceTime)
  => Math.Max(0L, raceTime - pressTime) * (long)pressTime;

IEnumerable<(int time, long record)> ParseInput(string[] input, bool single = false)
{
  var stripped = input.Select(i => i.Split(':', 2)[1]).ToArray();

  if (single)
    stripped = stripped.Select(x => x.Replace(" ", "")).ToArray();

  var time = stripped[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse);

  var dist = stripped[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(long.Parse);

  return time.Zip(dist);
}
var reports = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                  .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
                  .ToList();

var safeCount1 = reports.Count(IsSafe);

Console.WriteLine($"Part 1 Result = {safeCount1}");

var safeCount2 = reports.Count(x => IsSafe(x)
                                 || EnumRemoveOne(x).Any(IsSafe));

Console.WriteLine($"Part 2 Result = {safeCount2}");

static bool IsSafe(IEnumerable<int> input)
{
  var diffs = input.Zip(input.Skip(1)).Select(x => x.Second - x.First).ToArray();
  return diffs.Select(Math.Sign).Distinct().Count() == 1
      && diffs.All(n => Math.Abs(n) is >= 1 and <= 3);
}

static IEnumerable<IEnumerable<int>> EnumRemoveOne(IList<int> input)
{
  for (var i = 0; i < input.Count; ++i)
    yield return input.Where((n, j) => i != j);
}
var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt");

var top3 = EnumTotals(input).OrderByDescending(x => x).Take(3).ToArray();

Console.WriteLine($"Part1 Result = {top3.First()}");
Console.WriteLine($"Part2 Result = {top3.Sum()}");

IEnumerable<int> EnumTotals(IEnumerable<string> input)
{
  var sum = 0;
  foreach (var item in input)
  {
    if (item.Length == 0)
    {
      yield return sum;
      sum = 0;
    }
    else
    {
      sum += int.Parse(item);
    }
  }
}
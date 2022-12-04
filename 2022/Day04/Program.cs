var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .Select(line => line.Split(',', 2).Select(Range.Parse).ToArray())
                .ToList();

var fullyContainedCount = input.Count(x => x[0].Contains(x[1]) || x[1].Contains(x[0]));
Console.WriteLine($"Part 1 Result = {fullyContainedCount}");

var overlappingCount = input.Count(x => x[0].Overlaps(x[1]));
Console.WriteLine($"Part s Result = {overlappingCount}");

record Range(int Start, int End)
{
  public bool Contains(Range other)
    => Start <= other.Start && End >= other.End;

  public bool Overlaps(Range other)
    => Start <= other.End && End >= other.Start;

  public static Range Parse(string input)
  {
    var split = input.Split('-', 2);
    return new Range(
      int.Parse(split[0]),
      int.Parse(split[1]));
  }
}
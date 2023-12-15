var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(line => line.Split(','))
                .ToArray();

var result1 = input.Sum(HASH);
Console.WriteLine($"Part 1 Result = {result1}");

static int HASH(string input)
{
  return input.Aggregate(0, Step);

  int Step(int current, char ch)
    => ((current + (int)ch) * 17) % 256;
}
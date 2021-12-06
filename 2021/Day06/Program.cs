var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(x => x.Split(','))
                .Select(int.Parse)
                .ToList();

Console.WriteLine($"Part 1 Result = {CountFish(input, 80)}");
Console.WriteLine($"Part 2 Result = {CountFish(input, 256)}");

long CountFish(IEnumerable<int> input, int generations)
{
  var fish = new long[9];
  foreach (var age in input)
    fish[age]++;

  for (var i = 0; i < generations; ++i)
    fish[(i + 7) % 9] += fish[i % 9];

  return fish.Sum();
}
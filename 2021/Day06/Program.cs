var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .SelectMany(x => x.Split(','))
                .Select(int.Parse)
                .ToList();

var fish = new long[9];
foreach (var age in input)
  fish[age]++;

for (var i = 0; i < 256; ++i)
{
  var zeroIndex = (i % 9);
  var spawnAtIndex = ((i + 7) % 9);

  fish[spawnAtIndex] += fish[zeroIndex];

  if (i + 1 == 80)
    Console.WriteLine($"Part 1 Result = {fish.Sum()}");
}

Console.WriteLine($"Part 2 Result = {fish.Sum()}");

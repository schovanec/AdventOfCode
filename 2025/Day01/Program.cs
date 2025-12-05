var input = File.ReadLines(args.FirstOrDefault("input.txt"))
                .Select(Parse);

var result1 = EnumPositions(input).Count(n => n == 0);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = EnumAllPositions(input).Count(n => n == 0);
Console.WriteLine($"Part 2 Result = {result2}");

static IEnumerable<int> EnumPositions(IEnumerable<int> moves, int start = 50, int digits = 100)
{
  int current = start;
  foreach (var n in moves)
  {
    current = ((current + n) + digits) % digits;
    yield return current;
  }
}

static IEnumerable<int> EnumAllPositions(IEnumerable<int> moves, int start = 50, int digits = 100)
{
  int current = start;
  foreach (var n in moves)
  {
    for (int i = 0; i < Math.Abs(n); ++i)
    {
      current = ((current + Math.Sign(n)) + digits) % digits;
      yield return current;
    }
  }
}

static int Parse(string input)
  => input[0] switch
  {
    'L' => -int.Parse(input[1..]),
    'R' => int.Parse(input[1..]),
    _ => 0
  };
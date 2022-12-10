var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                       .Select(ParseInstruction)
                       .ToList();

DoPart1(input);
DoPart2(input);

static void DoPart1(IEnumerable<(string op, int? operand)> instructions)
{
  var signalStrengths = Execute(instructions).Select((x, i) => (tick: i + 1, x))
                                             .Where(a => (a.tick - 20) % 40 == 0)
                                             .Select(a => (a.tick, strength: a.x * a.tick));

  Console.WriteLine($"Part 1 Result = {signalStrengths.Sum(a => a.strength)}");
}

static void DoPart2(IEnumerable<(string op, int? operand)> instructions)
{
  const int cols = 40;
  const int rows = 6;
  var screen = new char[cols * rows];
  var output = Execute(instructions).Select((x, i) => (tick: i, x));
  foreach (var (tick, x) in output)
  {
    var index = tick % screen.Length;
    var col = index % cols;
    if (col >= x - 1 && col <= x + 1)
      screen[index] = '#';
    else
      screen[index] = '.';
  }

  Console.WriteLine("Part 2 Result:");
  for (var row = 0; row < rows; ++row)
    Console.WriteLine(new string(screen, row * cols, cols));
}

static IEnumerable<int> Execute(IEnumerable<(string op, int? operand)> instructions)
{
  var x = 1;
  foreach (var instruction in instructions)
  {
    switch (instruction)
    {
      case ("noop", _):
        yield return x;
        break;

      case ("addx", int value):
        yield return x;
        yield return x;
        x += value;
        break;
    }
  }
}

static (string op, int? operand) ParseInstruction(string instruction)
{
  var parts = instruction.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
  return (parts[0], parts.Length > 1 ? int.Parse(parts[1]) : default);
}
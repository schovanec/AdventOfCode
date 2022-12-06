using System.Collections.Immutable;

var (state, steps) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var currentPart1 = state;
var currentPart2 = state;
foreach (var step in steps)
{
  currentPart1 = currentPart1.Apply(step, preserveOrder: false);
  currentPart2 = currentPart2.Apply(step, preserveOrder: true);
}

Console.WriteLine($"Part 1 Result = {currentPart1.Tops}");
Console.WriteLine($"Part 1 Result = {currentPart2.Tops}");

static (State, ImmutableList<Step>) ParseInput(string[] input)
{
  var split = Array.FindIndex(input, string.IsNullOrEmpty);

  var stacks = ParseStacks(input[..split]);
  var steps = ParseSteps(input[(split + 1)..]);

  return (stacks, steps);
}

static State ParseStacks(string[] input)
{
  var positions = input.Last()
                       .Select((ch, index) => (index, ch))
                       .Where(x => char.IsDigit(x.ch))
                       .Select(x => (x.index, num: x.ch - '0'))
                       .ToArray();

  var result = positions.ToImmutableDictionary(x => x.num, _ => ImmutableStack<char>.Empty)
                        .ToBuilder();

  foreach (var line in input.Reverse().Skip(1))
  {
    foreach (var (index, num) in positions)
    {
      var ch = line[index];
      if (char.IsLetter(ch))
        result[num] = result[num].Push(ch);
    }
  }

  return new State(result.ToImmutable());
}

static ImmutableList<Step> ParseSteps(string[] input)
  => input.Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries))
          .Select(x => new Step(int.Parse(x[1]), int.Parse(x[3]), int.Parse(x[5])))
          .ToImmutableList();

record Step(int Count, int From, int To);

record State(ImmutableDictionary<int, ImmutableStack<char>> Stacks)
{
  public string Tops => new(Stacks.Values.Select(x => x.Peek()).ToArray());

  public State Apply(Step step, bool preserveOrder = false)
  {
    var from = Stacks[step.From];
    var to = Stacks[step.To];
    var count = step.Count;

    var (newFrom, newTo) = preserveOrder
      ? MoveInOrder(from, to, count)
      : Move(from, to, count);

    return new State(Stacks.SetItem(step.From, newFrom)
                           .SetItem(step.To, newTo));
  }

  static (ImmutableStack<char> from, ImmutableStack<char> to) Move(
    ImmutableStack<char> from,
    ImmutableStack<char> to,
    int count)
    => count switch
    {
      <= 0 => (from, to),
      _ => Move(from.Pop(out var ch), to.Push(ch), count - 1)
    };

  static (ImmutableStack<char> from, ImmutableStack<char> to) MoveInOrder(
    ImmutableStack<char> from,
    ImmutableStack<char> to,
    int count)
  {
    (from, var temp) = Move(from, ImmutableStack<char>.Empty, count);
    (_, to) = Move(temp, to, count);
    return (from, to);
  }
}
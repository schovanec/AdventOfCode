using System.Collections.Immutable;

var input = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt")).ToImmutableArray();

var result1 = CalculateMonkeyBusiness(input, 20, w => w / 3);
Console.WriteLine($"Part 1 Result = {result1}");

var wrap = input.Select(m => m.Test.Divisor).Aggregate((a, b) => a * b);
var result2 = CalculateMonkeyBusiness(input, 10000, w => w % wrap);
Console.WriteLine($"Part 2 Result = {result2}");

static long CalculateMonkeyBusiness(ImmutableArray<Monkey> initial, int rounds, Func<long, long> reduce)
{
  var state = initial;
  for (var i = 0; i < rounds; ++i)
    state = DoOneRound(state, reduce);

  return state.OrderByDescending(m => m.Inspections)
              .Take(2)
              .Select(m => m.Inspections)
              .Aggregate((a, b) => a * b);
}

static ImmutableArray<Monkey> DoOneRound(ImmutableArray<Monkey> monkeys, Func<long, long> reduce)
{
  var builder = monkeys.ToBuilder();
  for (var i = 0; i < builder.Count; ++i)
  {
    var monkey = builder[i];
    var throws = monkey.DetermineThrows(reduce);
    builder[i] = monkey.AfterInspections();

    foreach (var g in throws.GroupBy(x => x.target))
      builder[g.Key] = builder[g.Key].Catch(g.Select(x => x.worry));
  }

  return builder.ToImmutable();
}

static IEnumerable<Monkey> ParseInput(ArraySegment<string> input)
{
  var i = 0;
  while (i < input.Count)
  {
    if (input[i].StartsWith("Monkey "))
    {
      yield return ParseOneMonkey(input[i..], out var tail);
      input = tail;
      i = 0;
    }
    else
    {
      ++i;
    }
  }
}

static Monkey ParseOneMonkey(ArraySegment<string> input, out ArraySegment<string> tail)
{
  var id = int.Parse(input[0].Split(' ', 2)[1].TrimEnd(':'));
  var items = input[1].Split(':', 2)[1].Split(',', StringSplitOptions.TrimEntries).Select(long.Parse);
  var operation = ParseOperation(input[2].Split(':', StringSplitOptions.TrimEntries)[1]);
  var test = ParseTest(input[3..], out tail);
  return new Monkey(
    id,
    items.ToImmutableList(),
    operation,
    test);
}

static Func<long, long> ParseOperation(string input)
{
  var expression = input.Split('=', 2, StringSplitOptions.TrimEntries)[1];
  var parts = expression.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

  Func<long, long> GetOperand(string value) => value switch
  {
    "old" => (old => old),
    _ when long.TryParse(value, out var num) => (_ => num),
    _ => (_ => 0)
  };

  var left = GetOperand(parts[0]);
  var op = parts[1];
  var right = GetOperand(parts[2]);
  return op switch
  {
    "*" => (old => left(old) * right(old)),
    "+" => (old => left(old) + right(old)),
    _ => (_ => 0)
  };
}

static Test ParseTest(ArraySegment<string> input, out ArraySegment<string> tail)
{
  var divisor = int.Parse(input[0].Split(' ').Last());
  var monkeyIfTrue = int.Parse(input[1].Split(' ').Last());
  var monkeyIfFalse = int.Parse(input[2].Split(' ').Last());

  tail = input[3..];
  return new Test(divisor, monkeyIfTrue, monkeyIfFalse);
}

record Monkey(int Id, ImmutableList<long> Items, Func<long, long> Operation, Test Test, long Inspections = 0L)
{
  public Monkey AfterInspections()
    => this with { Inspections = Inspections + Items.Count, Items = ImmutableList<long>.Empty };

  public IEnumerable<(long worry, int target)> DetermineThrows(Func<long, long> reduce)
    => from w in Items
       let wp = Operation(w)
       let reduced = reduce(wp)
       select (reduced, Test.GetNextMoney(reduced));

  public Monkey Catch(IEnumerable<long> moreItems)
    => this with { Items = Items.AddRange(moreItems) };

  public override string ToString()
    => $"Monkey {Id}: [{string.Join(", ", Items)}] {{Inspections: {Inspections}}}";
}

record struct Test(int Divisor, int TrueMonkeyId, int FalseMonkeyId)
{
  public int GetNextMoney(long worry) => worry % Divisor == 0 ? TrueMonkeyId : FalseMonkeyId;
}
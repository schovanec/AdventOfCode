using System.Collections.Immutable;

var (initialState, program) = Parse(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var state = ExecuteLoop(initialState, program);
var result1 = string.Join(",", state.Output);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = Solve(initialState, program);
Console.WriteLine($"Part 2 Result = {result2}");

static long? Solve(State initial, ImmutableArray<int> program)
{
  return Search(Enumerable.Repeat(0, program.Length).ToImmutableArray(), 0);

  long? Search(ImmutableArray<int> coefs, int index)
  {
    if (index >= program.Length)
      return LongFromOctalCoefs(coefs);

    var outputIndex = program.Length - index - 1;
    for (var i = 0; i < 8; ++i)
    {
      var coefs_p = coefs.SetItem(index, i);
      var a = LongFromOctalCoefs(coefs_p);
      var end = ExecuteLoop(initial with { A = a }, program);
      if (end.Output.Count == program.Length && end.Output[outputIndex] == program[outputIndex])
      {
        if (Search(coefs_p, index + 1) is long result)
          return result;
      }
    }

    return default;
  }

  long LongFromOctalCoefs(ImmutableArray<int> coefs)
    => coefs.Aggregate(0L, (a, n) => (a << 3) + n);
}

static State ExecuteLoop(State initial, ImmutableArray<int> program, Func<State, bool>? shouldContinue = default)
{
  var state = initial;
  while (!state.IsHalted && (shouldContinue?.Invoke(state) ?? true))
    state = Execute(state, program);

  return state;
}

static State Execute(State state, ImmutableArray<int> program)
{
  if (state.ProgramCounter >= program.Length)
    return state.Halt();

  var opcode = (Opcode)program[state.ProgramCounter];
  var operand = program[state.ProgramCounter + 1];
  switch (opcode)
  {
    case Opcode.Adv:
      return state.Step() with { A = state.A / (1L << (int)state.ReadComboOperand(operand)) };

    case Opcode.Bxl:
      return state.Step() with { B = state.B ^ operand };

    case Opcode.Bst:
      return state.Step() with { B = state.ReadComboOperand(operand) % 8 };

    case Opcode.Jnz:
      if (state.A == 0)
        return state.Step();
      else
        return state with { ProgramCounter = operand };

    case Opcode.Bxc:
      return state.Step() with { B = state.B ^ state.C };

    case Opcode.Out:
      return state.Step() with { Output = state.Output.Add((int)(state.ReadComboOperand(operand) % 8)) };

    case Opcode.Bdv:
      return state.Step() with { B = state.A / (1L << (int)state.ReadComboOperand(operand)) };

    case Opcode.Cdv:
      return state.Step() with { C = state.A / (1L << (int)state.ReadComboOperand(operand)) };

    default:
      throw new InvalidOperationException();
  }
}

static (State state, ImmutableArray<int> program) Parse(IReadOnlyList<string> input)
{
  var a = int.Parse(input[0].Split(": ")[1]);
  var b = int.Parse(input[1].Split(": ")[1]);
  var c = int.Parse(input[2].Split(": ")[1]);

  var program = input[4].Split(": ")[1].Split(',').Select(int.Parse).ToImmutableArray();

  var state = State.Empty.Initialize(a, b, c);

  return (state, program);
}

enum Opcode { Adv, Bxl, Bst, Jnz, Bxc, Out, Bdv, Cdv }

record struct State(long A, long B, long C, int ProgramCounter, ImmutableList<int> Output, bool IsHalted = false)
{
  public static State Empty = new State(0, 0, 0, 0, ImmutableList<int>.Empty);

  public State Initialize(int a, int b, int c)
    => Empty with { A = a, B = b, C = c };

  public State Halt() => this with { IsHalted = true };

  public State Step() => this with { ProgramCounter = ProgramCounter + 2 };

  public long ReadComboOperand(long operand)
    => operand switch
    {
      >= 0 and <= 3 => operand,
      4 => A,
      5 => B,
      6 => C,
      _ => throw new InvalidOperationException()
    };
}

using System.Collections.Immutable;

var input = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var root = (BinaryOp)input["root"];

var result1 = root.Calculate(input.GetValueOrDefault).GetValue();
Console.WriteLine($"Part 1 Result = {result1}");

var input2 = input.Remove("humn");
var result2 = SolveEquality(root.Left, root.Right, input2.GetValueOrDefault);
Console.WriteLine($"Part 2 Result = {result2}");

static long SolveEquality(Expression lhs, Expression rhs, Func<string, Expression?> resolve)
  => (lhs.Calculate(resolve), rhs.Calculate(resolve)) switch
  {
    (UnknownResult u, ValueResult v) => Solve(u.Expression, v.Value),
    (ValueResult v, UnknownResult u) => Solve(u.Expression, v.Value),
    _ => throw new InvalidOperationException()
  };

static long Solve(Expression expr, long value)
  => expr switch
  {
    Reference => value,
    BinaryOp(Number n, '+', Expression rhs) => Solve(rhs, value - n.Value),
    BinaryOp(Number n, '-', Expression rhs) => Solve(rhs, n.Value - value),
    BinaryOp(Number n, '*', Expression rhs) => Solve(rhs, value / n.Value),
    BinaryOp(Number n, '/', Expression rhs) => Solve(rhs, n.Value / value),
    BinaryOp(Expression lhs, '+', Number n) => Solve(lhs, value - n.Value),
    BinaryOp(Expression lhs, '-', Number n) => Solve(lhs, value + n.Value),
    BinaryOp(Expression lhs, '*', Number n) => Solve(lhs, value / n.Value),
    BinaryOp(Expression lhs, '/', Number n) => Solve(lhs, value * n.Value),
    _ => throw new InvalidOperationException()
  };

static ImmutableDictionary<string, Expression> ParseInput(IEnumerable<string> input)
{
  var result = ImmutableDictionary.CreateBuilder<string, Expression>();

  foreach (var item in input)
  {
    var parts = item.Split(':', 2, StringSplitOptions.TrimEntries);
    result.Add(parts[0], Expression.Parse(parts[1]));
  }

  return result.ToImmutable();
}

abstract record Expression
{
  public abstract Result Calculate(Func<string, Expression?> resolve);

  public static Expression Parse(string input)
  {
    var parts = input.Split(' ');
    return parts.Length switch
    {
      1 => new Number(long.Parse(parts[0])),
      3 => new BinaryOp(new Reference(parts[0]), parts[1][0], new Reference((parts[2]))),
      _ => throw new FormatException()
    };
  }
}

record Number(long Value) : Expression
{
  public override Result Calculate(Func<string, Expression?> resolve) => new ValueResult(Value);

  public override string ToString() => Value.ToString();
}

record BinaryOp(Expression Left, char Operation, Expression Right) : Expression
{
  public override Result Calculate(Func<string, Expression?> resolve)
  {
    var lhs = Left.Calculate(resolve);
    var rhs = Right.Calculate(resolve);

    return (lhs, rhs) switch
    {
      (ValueResult a, ValueResult b) => CalculateValueResult(a.Value, b.Value),
      (UnknownResult, ValueResult) => new UnknownResult(new BinaryOp(lhs.GetExpression(), Operation, rhs.GetExpression())),
      (ValueResult, UnknownResult) => new UnknownResult(new BinaryOp(lhs.GetExpression(), Operation, rhs.GetExpression())),
      _ => throw new InvalidOperationException()
    };
  }

  private ValueResult CalculateValueResult(long lhs, long rhs)
    => Operation switch
    {
      '+' => new(lhs + rhs),
      '-' => new(lhs - rhs),
      '/' => new(lhs / rhs),
      '*' => new(lhs * rhs),
      _ => new(0)
    };

  public override string ToString() => $"({Left} {Operation} {Right})";
}

record Reference(string Name) : Expression
{
  public override Result Calculate(Func<string, Expression?> resolve)
  {
    var result = resolve(Name);
    return result != null
      ? result.Calculate(resolve)
      : new UnknownResult(this);
  }

  public override string ToString() => Name;
}

abstract record Result
{
  public long? GetValue() => this is ValueResult v ? v.Value : null;

  public Expression GetExpression()
    => this switch
    {
      UnknownResult u => u.Expression,
      _ => new Number(GetValue() ?? 0L)
    };
}

record ValueResult(long Value) : Result;

record UnknownResult(Expression Expression) : Result;
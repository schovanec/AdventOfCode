using System.Diagnostics.CodeAnalysis;

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(SnailfishNode.Parse)
                .ToList()
                .AsReadOnly();

var result1 = CalculateMagnitude(input.Aggregate(Add));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = EnumPairs(input).Select(x => Add(x.first, x.second))
                              .Max(CalculateMagnitude);
Console.WriteLine($"Part 2 Result = {result2}");

SnailfishNode Add(SnailfishNode first, SnailfishNode second)
 => Reduce(new PairNode(first, second));

SnailfishNode Reduce(SnailfishNode node)
{
  var result = node;
  while (TryReduce(result, out var reduced))
    result = reduced;

  return result;
}

bool TryReduce(SnailfishNode node, [NotNullWhen(true)] out SnailfishNode? reduced)
  => TryExplode(node, out reduced) || TrySplit(node, out reduced);

bool TryExplode(SnailfishNode node, [NotNullWhen(true)] out SnailfishNode? replacement)
  => TryExplodeInternal(node, 0, out replacement, out var exploded);

bool TryExplodeInternal(SnailfishNode node,
                        int depth,
                        [NotNullWhen(true)] out SnailfishNode? replacement,
                        out (int? left, int? right) exploded)
{
  switch (node)
  {
    case PairNode { Left: NumberNode a, Right: NumberNode b } when depth == 4:
      replacement = new NumberNode(0);
      exploded = (a.Value, b.Value);
      return true;

    case PairNode pair when TryExplodeInternal(pair.Left, depth + 1, out var left, out exploded):
      replacement = exploded.right is int rightValue
        ? new PairNode(left, AddToFirstNumber(pair.Right, rightValue))
        : pair with { Left = left };
      exploded.right = null;
      return true;

    case PairNode pair when TryExplodeInternal(pair.Right, depth + 1, out var right, out exploded):
      replacement = exploded.left is int leftValue
        ? new PairNode(AddToLastNumber(pair.Left, leftValue), right)
        : pair with { Right = right };
      exploded.left = null;
      return true;

    default:
      replacement = default;
      exploded = default;
      return false;
  }
}

bool TrySplit(SnailfishNode node, [NotNullWhen(true)] out SnailfishNode? replacement)
{
  switch (node)
  {
    case NumberNode number when number.Value >= 10:
      replacement = new PairNode(
        new NumberNode((int)Math.Floor(number.Value / 2.0m)),
        new NumberNode((int)Math.Ceiling(number.Value / 2.0m)));
      return true;

    case PairNode pair when TrySplit(pair.Left, out var left):
      replacement = pair with { Left = left };
      return true;

    case PairNode pair when TrySplit(pair.Right, out var right):
      replacement = pair with { Right = right };
      return true;

    default:
      replacement = default;
      return false;
  }
}

SnailfishNode AddToFirstNumber(SnailfishNode node, int value)
  => TryAddToFirstNumber(node, value, out var added) ? added : node;

bool TryAddToFirstNumber(SnailfishNode node, int value, [NotNullWhen(true)] out SnailfishNode? added)
{
  switch (node)
  {
    case NumberNode number:
      added = new NumberNode(number.Value + value);
      return true;

    case PairNode pair when TryAddToFirstNumber(pair.Left, value, out var left):
      added = new PairNode(left, pair.Right);
      return true;

    default:
      added = default;
      return false;
  }
}

SnailfishNode AddToLastNumber(SnailfishNode node, int value)
  => TryAddToLastNumber(node, value, out var added) ? added : node;

bool TryAddToLastNumber(SnailfishNode root, int value, [NotNullWhen(true)] out SnailfishNode? added)
{
  switch (root)
  {
    case NumberNode number:
      added = new NumberNode(number.Value + value);
      return true;

    case PairNode pair when TryAddToLastNumber(pair.Right, value, out var right):
      added = new PairNode(pair.Left, right);
      return true;

    default:
      added = default;
      return false;
  }
}

int CalculateMagnitude(SnailfishNode node)
  => node switch
  {
    NumberNode number => number.Value,
    PairNode(var left, var right) => (3 * CalculateMagnitude(left)) + (2 * CalculateMagnitude(right)),
    _ => 0
  };

IEnumerable<(T first, T second)> EnumPairs<T>(IReadOnlyList<T> input)
{
  for (var i = 0; i < input.Count - 1; ++i)
  {
    for (var j = i + 1; j < input.Count; ++j)
    {
      yield return (input[i], input[j]);
      yield return (input[j], input[i]);
    }
  }
}

abstract record SnailfishNode
{
  public static SnailfishNode Parse(string text)
    => ParseInternal(text, out var tail);

  private static SnailfishNode ParseInternal(ReadOnlySpan<char> input, out ReadOnlySpan<char> tail)
  {
    if (input.Length > 0)
    {
      if (char.IsDigit(input[0]))
      {
        var end = input.IndexOfAny('[', ']', ',');
        var value = int.Parse(input.Slice(0, end));

        tail = input.Slice(end);
        return new NumberNode(value);
      }
      else if (input[0] == '[')
      {
        var left = ParseInternal(input.Slice(1), out input);
        var right = ParseInternal(input.Slice(1), out input);
        tail = input.Slice(1);  // skip trailing ']'
        return new PairNode(left, right);
      }
    }

    throw new FormatException();
  }
}

record NumberNode(int Value) : SnailfishNode { }

record PairNode(SnailfishNode Left, SnailfishNode Right) : SnailfishNode { }

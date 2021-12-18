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
  => TryExplodeInternal(node, 0, out replacement, out _);

bool TryExplodeInternal(SnailfishNode node,
                        int depth,
                        [NotNullWhen(true)] out SnailfishNode? replacement,
                        out (int? left, int? right) exploded)
{
  switch (node)
  {
    case PairNode(NumberNode(var left), NumberNode(var right)) when depth == 4:
      replacement = new NumberNode(0);
      exploded = (left, right);
      return true;

    case PairNode(var left, var right) when TryExplodeInternal(left, depth + 1, out left, out exploded):
      if (exploded.right is int rightValue)
      {
        right = AddToFirstNumber(right, rightValue);
        exploded.right = null;
      }

      replacement = new PairNode(left, right);
      return true;

    case PairNode(var left, var right) when TryExplodeInternal(right, depth + 1, out right, out exploded):
      if (exploded.left is int leftValue)
      {
        left = AddToLastNumber(left, leftValue);
        exploded.left = null;
      }

      replacement = new PairNode(left, right);
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
    case NumberNode(var value) when value >= 10:
      replacement = new PairNode(
        new NumberNode((int)Math.Floor(value / 2.0m)),
        new NumberNode((int)Math.Ceiling(value / 2.0m)));
      return true;

    case PairNode(var left, var right) pair when TrySplit(left, out left):
      replacement = new PairNode(left, right);
      return true;

    case PairNode(var left, var right) pair when TrySplit(right, out right):
      replacement = new PairNode(left, right);
      return true;

    default:
      replacement = default;
      return false;
  }
}

SnailfishNode AddToFirstNumber(SnailfishNode node, int amountToAdd)
  => TryAddToFirstNumber(node, amountToAdd, out var replacement) ? replacement : node;

bool TryAddToFirstNumber(SnailfishNode node,
                         int amountToAdd,
                         [NotNullWhen(true)] out SnailfishNode? replacement)
{
  switch (node)
  {
    case NumberNode(var value):
      replacement = new NumberNode(value + amountToAdd);
      return true;

    case PairNode(var left, var right) when TryAddToFirstNumber(left, amountToAdd, out left):
      replacement = new PairNode(left, right);
      return true;

    default:
      replacement = default;
      return false;
  }
}

SnailfishNode AddToLastNumber(SnailfishNode node, int amountToAdd)
  => TryAddToLastNumber(node, amountToAdd, out var replacement) ? replacement : node;

bool TryAddToLastNumber(SnailfishNode root,
                        int amountToAdd,
                        [NotNullWhen(true)] out SnailfishNode? replacement)
{
  switch (root)
  {
    case NumberNode(var value):
      replacement = new NumberNode(value + amountToAdd);
      return true;

    case PairNode(var left, var right) when TryAddToLastNumber(right, amountToAdd, out right):
      replacement = new PairNode(left, right);
      return true;

    default:
      replacement = default;
      return false;
  }
}

int CalculateMagnitude(SnailfishNode node)
  => node switch
  {
    NumberNode(var value) => value,
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

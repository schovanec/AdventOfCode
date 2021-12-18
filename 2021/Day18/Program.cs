var input = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt");

var result1 = input.Select(Node.Parse).Aggregate(AddNodes);
Console.WriteLine($"Part 1 Result = {result1?.GetMagnitude()}");

var result2 = EnumPairs(input).Max(x => AddSnailfishNumbers(x.first, x.second).GetMagnitude());
Console.WriteLine($"Part 2 Result = {result2}");

IEnumerable<(string first, string second)> EnumPairs(IList<string> input)
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

Node AddSnailfishNumbers(string first, string second)
  => AddNodes(Node.Parse(first), Node.Parse(second));

Node AddNodes(Node first, Node second)
{
  var result = Node.MakePair(first, second);
  Reduce(result);
  return result;
}

void Reduce(Node root)
{
  while (TryExplode(root) || TrySplit(root))
  { }
}

bool TryExplode(Node? node, int depth = 1)
{
  switch (node)
  {
    case PairNode { Left: PairNode target } pair when depth == 4:
      ExplodeNode(target);
      pair.Left = new NumberNode(0, pair);
      return true;

    case PairNode pair when TryExplode(pair.Left, depth + 1):
      return true;

    case PairNode { Right: PairNode target } pair when depth == 4:
      ExplodeNode(target);
      pair.Right = new NumberNode(0, pair);
      return true;

    case PairNode pair when TryExplode(pair.Right, depth + 1):
      return true;

    default:
      return false;
  }
}

bool TrySplit(Node? node)
{
  switch (node)
  {
    case PairNode { Left: NumberNode number } pair when number.Value >= 10:
      pair.Left = SplitNode(number);
      return true;

    case PairNode pair when TrySplit(pair.Left):
      return true;

    case PairNode { Right: NumberNode number } pair when number.Value >= 10:
      pair.Right = SplitNode(number);
      return true;

    case PairNode pair when TrySplit(pair.Right):
      return true;

    default:
      return false;
  }
}

void ExplodeNode(PairNode target)
{
  if (target.GetPreviousNumber() is NumberNode prev)
    prev.Value += target.GetFirstNumber()?.Value ?? 0;

  if (target.GetNextNumber() is NumberNode next)
    next.Value += target.GetLastNumber()?.Value ?? 0;
}

PairNode SplitNode(NumberNode number)
  => Node.MakePair(
      new NumberNode((int)Math.Floor(number.Value / 2.0m)),
      new NumberNode((int)Math.Ceiling(number.Value / 2.0m)),
      number.Parent);


PairNode? FindPair(Node? root, int depth)
  => root switch
  {
    PairNode pair when depth == 0 => pair,
    PairNode pair => FindPair(pair.Left, depth - 1) ?? FindPair(pair.Right, depth - 1),
    _ => null
  };

abstract class Node
{
  protected Node(Node? parent = null)
  {
    Parent = parent;
  }

  public Node? Parent { get; set; }

  public abstract int GetMagnitude();

  public abstract NumberNode? GetPreviousNumber(Node? relativeToChild = null);

  public abstract NumberNode? GetNextNumber(Node? relativeToChild = null);

  public abstract NumberNode? GetFirstNumber();

  public abstract NumberNode? GetLastNumber();

  public static PairNode MakePair(Node left, Node right, Node? parent = null)
  {
    var result = new PairNode(left, right, parent);
    left.Parent = result;
    right.Parent = result;
    return result;
  }

  public static Node Parse(string text)
    => ParseInternal(text, out var tail);

  private static Node ParseInternal(ReadOnlySpan<char> input, out ReadOnlySpan<char> tail)
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

        if (left != null && right != null)
          return MakePair(left, right);
      }
    }

    throw new FormatException();
  }
}

class NumberNode : Node
{
  public NumberNode(int value, Node? parent = null) : base(parent)
  {
    Value = value;
  }

  public int Value { get; set; }

  public override int GetMagnitude() => Value;

  public override string ToString() => Value.ToString();

  public override NumberNode? GetPreviousNumber(Node? relativeToChild = null)
    => Parent?.GetPreviousNumber(this) ?? this;

  public override NumberNode? GetNextNumber(Node? relativeToChild = null)
    => Parent?.GetNextNumber(this) ?? this;

  public override NumberNode? GetFirstNumber() => this;

  public override NumberNode? GetLastNumber() => this;
}

class PairNode : Node
{
  public PairNode(Node left, Node right, Node? parent = null) : base(parent)
  {
    Left = left;
    Right = right;
  }

  public Node Left { get; set; }

  public Node Right { get; set; }

  public override int GetMagnitude()
    => (3 * Left.GetMagnitude()) + (2 * Right.GetMagnitude());

  public override string ToString() => $"[{Left},{Right}]";

  public override NumberNode? GetPreviousNumber(Node? relativeToChild = null)
    => relativeToChild == Right
      ? Left?.GetLastNumber() ?? Parent?.GetPreviousNumber(this)
      : Parent?.GetPreviousNumber(this);

  public override NumberNode? GetNextNumber(Node? relativeToChild = null)
    => relativeToChild == Left
      ? Right?.GetFirstNumber() ?? Parent?.GetNextNumber(this)
      : Parent?.GetNextNumber(this);

  public override NumberNode? GetFirstNumber()
    => Left?.GetFirstNumber();

  public override NumberNode? GetLastNumber()
    => Right?.GetLastNumber();
}

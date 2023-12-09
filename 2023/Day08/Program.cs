var (steps, nodes) = ParseInput(File.ReadAllLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = CountSteps("AAA", id => id == "ZZZ", steps, nodes);
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = nodes.Where(n => n.Key.EndsWith('A'))
                   .Select(n => CountSteps(n.Key, id => id.EndsWith('Z'), steps, nodes))
                   .Aggregate(LCM);
Console.WriteLine($"Part 2 Result = {result2}");

long CountSteps(string start, Func<string, bool> isGoal, string steps, Dictionary<string, Node> nodes)
{
  var current = nodes[start];
  var count = 0L;
  while (!isGoal(current.Id))
  {
    var dir = steps[(int)(count % steps.Length)];
    var next = dir == 'L' ? current.Left : current.Right;
    current = nodes[next];
    ++count;
  }

  return count;
}

long LCM(long a, long b) => (a * b) / GCD(a, b);

long GCD(long a, long b) => a % b == 0 ? b : GCD(b, a % b);

(string steps, Dictionary<string, Node> nodes) ParseInput(string[] input)
{
  var steps = input[0];

  var nodes = input.Skip(2)
                   .Select(Node.Parse)
                   .ToDictionary(n => n.Id);

  return (steps, nodes);
}

record Node(string Id, string Left, string Right)
{
  public static Node Parse(string input)
  {
    var split = input.Split("=", 2, StringSplitOptions.TrimEntries);

    var edges = split[1].Trim('(', ')')
                        .Split(',', StringSplitOptions.TrimEntries);

    return new(split[0], edges[0], edges[1]);
  }
}
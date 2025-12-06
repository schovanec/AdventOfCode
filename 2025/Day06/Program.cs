using System.Collections.Immutable;

var input = ParseInput(File.ReadAllLines(args.FirstOrDefault("input.txt")));

var result1 = input.Sum(x => Compute(x.op, x.nums1));
Console.WriteLine($"Part 1 Result = {result1}");

var result2 = input.Sum(x => Compute(x.op, x.nums2));
Console.WriteLine($"Part 2 Result = {result2}");

static long Compute(char op, IEnumerable<long> numbers)
  => op switch
  {
    '+' => numbers.Sum(),
    '*' => numbers.Aggregate(1L, (a, b) => a * b),
    _ => 0
  };

static IEnumerable<(char op, ImmutableArray<long> nums1, ImmutableArray<long> nums2)> ParseInput(string[] input)
{
  var numLines = input[..1];

  List<(char op, int start, int end)> columns = new();
  var opsLine = input.Last();
  var pos = 0;
  while (pos < opsLine.Length)
  {
    var op = opsLine[pos];
    var next = opsLine.IndexOfAny(['+','*'], pos + 1);
    if (next < 0)
      next = next = opsLine.Length + 1;

    columns.Add((op, pos, next - 1));
    pos = next;
  }

  var nums = input[..^1];
  foreach (var col in columns)
  {
    var nums1 = nums.Select(line => long.Parse(line[col.start..col.end]))
                    .ToImmutableArray();

    var nums2 = ImmutableArray.CreateBuilder<long>();
    for (var i = col.start; i < col.end; ++i)
    {
      var n = 0L;
      foreach (var ch in nums.Select(line => line[i]))
      {
        if (char.IsDigit(ch))
          n = (n * 10) + (ch - '0');
      }
      nums2.Add(n);
    }

    yield return (col.op, nums1, nums2.ToImmutable());
  }
}
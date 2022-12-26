var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(long.Parse)
                .ToList();

var result1 = MixAndGetCoordinates(input, 1);
Console.WriteLine($"Part 1 Result = {result1.Sum()}");

const long key = 811589153L;
var result2 = MixAndGetCoordinates(input.Select(n => n * key), 10);
Console.WriteLine($"Part 2 Result = {result2.Sum()}");

static long[] MixAndGetCoordinates(IEnumerable<long> input, int times)
{
  var list = new LinkedList<long>(input);
  Mix(list, times);
  return GetGroveCoordinates(list).ToArray();
}

static void Mix(LinkedList<long> list, int times = 1)
{
  var nodes = EnumLinkedListNodes(list).ToList();
  while (times > 0)
  {
    --times;

    foreach (var node in nodes)
    {
      var offset = node.Value;
      if (offset > 0)
      {
        var next = NextWrap(list, node);
        list.Remove(node);
        var insertBefore = NextWrap(list, next, offset);
        list.AddBefore(insertBefore!, node);
      }
      else if (offset < 0)
      {
        var prev = PreviousWrap(list, node);
        list.Remove(node);
        var insertAfter = PreviousWrap(list, prev, -offset);
        list.AddAfter(insertAfter!, node);
      }
    }
  }
}

static LinkedListNode<long> NextWrap(LinkedList<long> list, LinkedListNode<long> node, long count = 1)
{
  count %= list.Count;

  while (count > 0)
  {
    --count;
    node = node.Next ?? list.First ?? throw new InvalidOperationException();
  }

  return node;
}

static LinkedListNode<long> PreviousWrap(LinkedList<long> list, LinkedListNode<long> node, long count = 1)
{
  count %= list.Count;

  while (count > 0)
  {
    --count;
    node = node.Previous ?? list.Last ?? throw new InvalidOperationException();
  }

  return node;
}

static IEnumerable<LinkedListNode<long>> EnumLinkedListNodes(LinkedList<long> list)
{
  var current = list.First;
  while (current != null)
  {
    yield return current;
    current = current.Next;
  }
}

static IEnumerable<long> GetGroveCoordinates(LinkedList<long> list)
{
  var start = list.Find(0);
  if (start != null)
  {
    for (var i = 0; i < 3; ++i)
    {
      start = NextWrap(list, start, 1000);
      yield return start.Value;
    }
  }
}
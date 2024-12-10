using System.Diagnostics.CodeAnalysis;

var fileMap = File.ReadAllLines(args.FirstOrDefault() ?? "input.txt").First();

var list1 = new LinkedList<Block>(ParseFileMap(fileMap));
CompactMethod1(list1);
var result1 = CalcChecksum(list1);
Console.WriteLine($"Part 1 Result = {result1}");

var list2 = new LinkedList<Block>(ParseFileMap(fileMap));
CompactMethod2(list2);
var result2 = CalcChecksum(list2);
Console.WriteLine($"Part 2 Result = {result2}");

static void CompactMethod1(LinkedList<Block> blocks)
{
  var firstFree = FindForward(blocks.First, b => b.IsFree);
  var lastFile = FindReverse(blocks.Last, b => b.IsFile && b.Size > 0);

  while (firstFree != null && firstFree.List == blocks && lastFile != null)
  {
    if (firstFree.Value.Size <= lastFile.Value.Size)
    {
      firstFree.Value = firstFree.Value with { Id = lastFile.Value.Id };
      lastFile.Value = lastFile.Value with { Size = lastFile.Value.Size - firstFree.Value.Size };
    }
    else
    {
      blocks.AddAfter(firstFree, new Block(firstFree.Value.Offset + lastFile.Value.Size, firstFree.Value.Size - lastFile.Value.Size));
      firstFree.Value = lastFile.Value with { Offset = firstFree.Value.Offset };
      lastFile.Value = lastFile.Value with { Size = 0 };
    }

    firstFree = FindForward(firstFree, b => b.IsFree);
    lastFile = FindReverse(lastFile, b => b.IsFile && b.Size > 0);

    while (blocks.Last != lastFile)
      blocks.RemoveLast();
  }
}

static void CompactMethod2(LinkedList<Block> blocks)
{
  var lastFile = FindReverse(blocks.Last, b => b.IsFile);

  while (lastFile != null)
  {
    var freeBlock = FindForward(blocks.First, b => b.IsFree && b.Size >= lastFile.Value.Size && b.Offset < lastFile.Value.Offset);
    if (freeBlock != null)
    {
      if (freeBlock.Value.Size > lastFile.Value.Size)
        blocks.AddAfter(freeBlock, new Block(freeBlock.Value.Offset + lastFile.Value.Size, freeBlock.Value.Size - lastFile.Value.Size));

      freeBlock.Value = lastFile.Value with { Offset = freeBlock.Value.Offset };
      lastFile.Value = lastFile.Value with { Id = null };
    }

    lastFile = FindReverse(lastFile.Previous, b => b.IsFile);
  }
}

static long CalcChecksum(IEnumerable<Block> blocks) => blocks.Sum(b => b.CalcChecksum());


static LinkedListNode<Block>? FindForward(LinkedListNode<Block>? node, Func<Block, bool> predicate)
{
  while (node != null)
  {
    if (predicate(node.Value))
      return node;

    node = node.Next;
  }

  return null;
}

static LinkedListNode<Block>? FindReverse(LinkedListNode<Block>? node, Func<Block, bool> predicate)
{
  while (node != null)
  {
    if (predicate(node.Value))
      return node;

    node = node.Previous;
  }

  return null;
}

static IEnumerable<Block> ParseFileMap(string map)
{
  var isFile = true;
  var fileId = 0;
  var offset = 0L;
  foreach (var n in map.Select(ch => ch - '0'))
  {
    if (isFile)
      yield return new(offset, n, fileId++);
    else
      yield return new(offset, n);

    isFile = !isFile;
    offset += n;
  }
}

record struct Block(long Offset, int Size, int? Id = default)
{
  [MemberNotNullWhen(true, nameof(Id))]
  public bool IsFile => Id.HasValue;

  public bool IsFree => !Id.HasValue;

  public long CalcChecksum()
  {
    var result = 0L;

    if (Id.HasValue)
    {
      for (var i = 0; i < Size; ++i)
        result += (Offset + i) * Id.Value;
    }

    return result;
  }
}
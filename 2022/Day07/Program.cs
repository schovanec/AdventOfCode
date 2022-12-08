var root = ParseInput(File.ReadLines(args.FirstOrDefault() ?? "input.txt"));

var result1 = (from d in root.EnumAllDirectories()
               let size = d.TotalSize
               where size <= 100000
               select size).Sum();

Console.WriteLine($"Part 1 Result = {result1}");

const long TotalSpace = 70000000;
const long TargetSpace = 30000000;
var used = root.TotalSize;
var required = TargetSpace - (TotalSpace - used);

var directoryToDeleteSize = (from d in root.EnumAllDirectories()
                             let size = d.TotalSize
                             where size >= required
                             orderby size
                             select size).First();

Console.WriteLine($"Part 2 Result = {directoryToDeleteSize}");

static DirectoryEntry ParseInput(IEnumerable<string> input)
{
  var root = new DirectoryEntry();

  var stack = new Stack<DirectoryEntry>();
  stack.Push(root);

  foreach (var line in input.Skip(1))
  {
    if (line.StartsWith("$"))
    {
      if (line == "$ cd ..")
      {
        stack.Pop();
      }
      else if (line.StartsWith("$ cd "))
      {
        var name = line[5..];
        stack.Push(stack.Peek().Children[name]);
      }
    }
    else
    {
      var current = stack.Peek();
      var parts = line.Split(' ', 2);
      if (parts[0] == "dir")
        current.Children[parts[1]] = new();
      else
        current.SizeOfFiles += long.Parse(parts[0]);
    }
  }

  return root;
}

class DirectoryEntry
{
  public Dictionary<string, DirectoryEntry> Children { get; } = new();

  public long SizeOfFiles { get; set; } = 0;

  public long TotalSize => SizeOfFiles + Children.Values.Sum(d => d.TotalSize);

  public IEnumerable<DirectoryEntry> EnumAllDirectories()
  {
    var queue = new Queue<DirectoryEntry>();
    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var current = queue.Dequeue();
      yield return current;

      foreach (var child in current.Children.Values)
        queue.Enqueue(child);
    }
  }
}
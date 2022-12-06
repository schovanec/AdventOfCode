var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt").First();

var packetMarkerPosition = FindMarkers(input, 4).First();
Console.WriteLine($"Part 1 Result = {packetMarkerPosition}");

var messageMarkerPosition = FindMarkers(input, 14).First();
Console.WriteLine($"Part 2 Result = {messageMarkerPosition}");

static IEnumerable<int> FindMarkers(string input, int size)
  => from i in Enumerable.Range(size, input.Length - size)
     let prev = Enumerable.Range(i - size, size).Select(n => input[n])
     where prev.Distinct().Count() == size
     select i;
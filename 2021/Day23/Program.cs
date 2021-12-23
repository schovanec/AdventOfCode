using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

var energy1 = SolveMinimumEnergyForFile(args.FirstOrDefault() ?? "input1.txt");
Console.WriteLine($"Part 1 Result = {energy1}");

var energy2 = SolveMinimumEnergyForFile(args.Skip(1).FirstOrDefault() ?? "input2.txt");
Console.WriteLine($"Part 1 Result = {energy2}");

int SolveMinimumEnergyForFile(string file)
{
  var input = Parse(File.ReadAllLines(file));
  var goal = input with { Rooms = new string(input.Rooms.OrderBy(ch => ch).ToArray()) };

  return SolveMinimumEnergy(input, goal);
}

int SolveMinimumEnergy(State start, State goal)
{
  var dist = new Dictionary<State, int>() { [start] = 0 };
  var visited = new HashSet<State>();
  var queue = new PriorityQueue<State, int>();

  queue.EnqueueRange(dist.Select(x => (x.Key, x.Value)));

  while (queue.Count > 0)
  {
    var current = queue.Dequeue();
    if (!visited.Contains(current))
    {
      visited.Add(current);

      if (current == goal)
        break;

      var currentCost = dist[current];

      foreach (var (next, nextCost) in current.EnumMoves())
      {
        var alt = currentCost + nextCost;
        if (alt < dist.GetValueOrDefault(next, int.MaxValue))
        {
          dist[next] = alt;
          queue.Enqueue(next, alt);
        }
      }
    }
  }

  return dist[goal];
}

State Parse(IReadOnlyList<string> input)
{
  var hall = new string('.', State.HallwayLength);
  var rooms = new[] { "", "", "", "" };

  foreach (var line in input)
  {
    if (line.Any(ch => State.AmphipodTypes.Contains(ch)))
    {
      var values = line.Where(ch => char.IsLetter(ch))
                       .Select(ch => ch.ToString())
                       .ToArray();

      for (var i = 0; i < rooms.Length; ++i)
        rooms[i] = (rooms[i] ?? "") + values[i];
    }
  }

  var roomsValue = string.Concat(rooms);
  return new State(hall, roomsValue, rooms[0].Length);
}

record State(string Hall, string Rooms, int RoomSize)
{
  public static ImmutableArray<int> RoomPositions = ImmutableArray.Create(2, 4, 6, 8);

  public static char[] AmphipodTypes = new[] { 'A', 'B', 'C', 'D' };

  public const int HallwayLength = 11;

  public string GetRoom(int index)
    => Rooms.Substring(index * RoomSize, RoomSize);

  public IEnumerable<(State state, int cost)> EnumMoves()
  {
    for (var i = 0; i < 4; ++i)
    {
      foreach (var pos in EnumOpenSpaces(i))
      {
        if (TryMoveOut(i, pos, out var updated, out var amphipod, out var steps))
        {
          var cost = steps * GetEnergyFactor(amphipod);
          yield return (updated, cost);
        }
      }
    }

    for (var i = 0; i < Hall.Length; ++i)
    {
      if (TryMoveIn(i, out var updated, out var amphipod, out var steps))
      {
        var cost = steps * GetEnergyFactor(amphipod);
        yield return (updated, cost);
      }
    }
  }

  private bool TryMoveOut(int roomIndex,
                          int targetPosition,
                          [NotNullWhen(true)] out State? updated,
                          out char amphipod,
                          out int steps)
  {
    var room = GetRoom(roomIndex);
    var depth = room.IndexOfAny(AmphipodTypes);
    if (depth < 0)
    {
      updated = default;
      amphipod = default;
      steps = default;
      return false;
    }

    steps = Math.Abs(targetPosition - RoomPositions[roomIndex]) + depth + 1;
    amphipod = room[depth];

    var updatedHall = Hall.Remove(targetPosition, 1).Insert(targetPosition, amphipod.ToString());
    var updatedRoom = room.Remove(depth, 1).Insert(depth, ".");

    updated = Update(updatedHall, roomIndex, updatedRoom);
    return true;
  }

  private bool TryMoveIn(int hallwayPosition,
                         [NotNullWhen(true)] out State? updated,
                         out char amphipod,
                         out int steps)
  {
    updated = default;
    steps = default;

    amphipod = Hall[hallwayPosition];
    var goalRoomIndex = Array.IndexOf(AmphipodTypes, amphipod);
    if (goalRoomIndex < 0)
      return false;

    var goalPosition = RoomPositions[goalRoomIndex];
    var start = goalPosition > hallwayPosition ? hallwayPosition + 1 : hallwayPosition - 1;
    var min = Math.Min(goalPosition, start);
    var max = Math.Max(goalPosition, start);
    if (Hall.Skip(min).Take(max - min + 1).Any(ch => ch != '.'))
      return false;

    var room = GetRoom(goalRoomIndex);
    var type = amphipod;
    if (room.Any(ch => ch != '.' && ch != type))
      return false;

    var depth = room.LastIndexOf('.');
    steps = (max - min + 1) + depth + 1;

    var updatedHall = Hall.Remove(hallwayPosition, 1).Insert(hallwayPosition, ".");
    var updatedRoom = room.Remove(depth, 1).Insert(depth, amphipod.ToString());

    updated = Update(updatedHall, goalRoomIndex, updatedRoom);
    return true;
  }

  private State Update(string updatedHall, int roomIndex, string updatedRoom)
  {
    var updatedRooms = Rooms.Remove(roomIndex * RoomSize, RoomSize).Insert(roomIndex * RoomSize, updatedRoom);
    return this with { Hall = updatedHall, Rooms = updatedRooms };
  }

  private IEnumerable<int> EnumOpenSpaces(int roomIndex)
  {
    var position = RoomPositions[roomIndex];

    for (var i = position - 1; i >= 0 && Hall[i] == '.'; --i)
    {
      if (!RoomPositions.Contains(i))
        yield return i;
    }

    for (var i = position + 1; i < Hall.Length && Hall[i] == '.'; ++i)
    {
      if (!RoomPositions.Contains(i))
        yield return i;
    }
  }

  private static int GetEnergyFactor(char amphipod)
    => amphipod switch
    {
      'A' => 1,
      'B' => 10,
      'C' => 100,
      'D' => 1000,
      _ => throw new ArgumentOutOfRangeException(nameof(amphipod))
    };
}

var input = File.ReadLines(args.FirstOrDefault() ?? "input.txt")
                .Select(int.Parse)
                .ToList();

var count1 = input.Zip(input.Skip(1))
                  .Count(x => x.First < x.Second);

Console.WriteLine($"Part 1 Result = {count1}");

var sums = input.Zip(input.Skip(1), input.Skip(2))
                .Select(x => x.First + x.Second + x.Third)
                .ToList();

var count2 = sums.Zip(sums.Skip(1))
                 .Count(x => x.First < x.Second);

Console.WriteLine($"Part 2 Result = {count2}");

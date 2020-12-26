using System;
using System.IO;
using System.Linq;

namespace Day25
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args.DefaultIfEmpty("input.txt").First();
            var publicKeys = File.ReadLines(file)
                                 .Select(int.Parse)
                                 .ToArray();

            var loopSize = publicKeys.Select(FindLoopCount).ToArray();

            foreach (var count in loopSize)
                Console.WriteLine($"Count = {count}");

            var key0 = FindKey(publicKeys[1], loopSize[0]);
            var key1 = FindKey(publicKeys[0], loopSize[1]);

            Console.WriteLine($"Key0 = {key0}");
            Console.WriteLine($"Key1 = {key1}");
        }

        static int FindLoopCount(int key)
        {
            var subject = 7L;
            var value = 1L;
            var count = 0;
            while (value != key)
            {
                value = Transform(subject, value);
                ++count;
            }

            return count;
        }

        static long FindKey(long publicKey, int loopCount)
        {
            var value = 1L;
            for (var i = 0; i < loopCount; ++i)
                value = Transform(publicKey, value);

            return value;
        }

        static long Transform(long subject, long value)
        {
            value *= subject;
            value %= 20201227;
            return value;
        }
    }
}

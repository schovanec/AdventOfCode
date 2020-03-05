using System;
using System.Linq;

namespace day04
{
    class Program
    {
        static void Main(string[] args)
        {
#if true
            var rangeFirst = 136760;
            var rangeLast = 595730;

            Console.WriteLine(Enumerable.Range(rangeFirst, rangeLast - rangeFirst + 1).Last());

            var count = Enumerable.Range(rangeFirst, rangeLast - rangeFirst + 1)
                                  .Select(i => i.ToString())
                                  .Count(IsPossiblePassword);

            Console.WriteLine($"Result = {count}");
#else
            var tests = new[]
            {
                new { Word = "111111", Expected = false },
                new { Word = "223450", Expected = false },
                new { Word = "123789", Expected = false },
                new { Word = "112233", Expected = true },
                new { Word = "123444", Expected = false },
                new { Word = "111122", Expected = true }
            };

            foreach (var test in tests)
            {
                var result = IsPossiblePassword(test.Word);
                Console.WriteLine($"Word = {test.Word}, Result = {result} - Pass?: {result == test.Expected}");
            }
#endif
        }

        private static bool IsPossiblePassword(string word)
        {
            if (word.Any(ch => !char.IsDigit(ch)))
                return false;

            var foundPair = false;
            var runLength = 1;
            for (var i = 1; i < word.Length; ++i)
            {
                var curr = word[i];
                var prev = word[i - 1];

                if (curr < prev)
                    return false;

                if (prev == curr)
                {
                    ++runLength;
                }
                else
                {
                    if (runLength == 2)
                        foundPair = true;

                    runLength = 1;
                }
            }

            return foundPair || runLength == 2;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day01
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = args.FirstOrDefault() ?? "input.txt";

#if true
            var totalFuelQuantity = ReadInput(fileName).Select(CalculateFuelQuantity).Sum();

            Console.WriteLine($"Total Fuel: {totalFuelQuantity}");
#else
            var tests = new (decimal mass, decimal fuel)[] {
                (12, 2),
                (14, 2),
                (1969, 966),
                (100756, 50346)
            };

            foreach (var item in tests)
            {
                var fuel = CalculateFuelQuantity(item.mass);
                Console.WriteLine($"Mass = {item.mass}, Expected = {item.fuel}, Actual = {fuel}, Pass? = {item.fuel == fuel}");
            }
#endif
        }

        private static decimal CalculateFuelQuantity(decimal mass)
        {
            var fuel = CalculateRawFuelQuantity(mass);
            if (fuel <= 0)
                return 0;

            return fuel + CalculateFuelQuantity(fuel);
        }

        private static decimal CalculateRawFuelQuantity(decimal mass)
            => Math.Floor(mass / 3m) - 2m;

        private static IEnumerable<decimal> ReadInput(string fileName)
        {
            using var reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (decimal.TryParse(line, out var result))
                    yield return result;
            }
        }
    }
}

using System;
using GZiper.Core;

namespace GZiper {
    internal static class Program {
        private static void Main(string[] args) {
            try {
                if (args.Length == 0 || args.Length > 3)
                    throw new ArgumentException("Not enough or too many arguments!");
                switch (args[0].ToLower()) {
                    case "compress":
                        Ziper.Compress(args[1], args[2]);
                        break;
                    case "decompress":
                        Ziper.Decompress(args[1], args[2]);
                        break;
                    default:
                        throw new ArgumentException("Unsupported method! use \"compress\" or \"decompress\".");
                }
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong in: ");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("With message: {0}", e.Message);
            }
            Console.WriteLine("Press any key to exit...");
        }
    }
}
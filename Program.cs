using System;
using GZiper.Core;

namespace GZiper {
    class Program {
        static void Main(string[] args) {
            string sourceFile = "u12.jpg"; // исходный файл
            string compressedFile = "test2.gz"; // сжатый файл
            string newFile = "u13.jpg";


             Ziper.Compress(sourceFile, compressedFile);
          Ziper.Decompress(compressedFile, newFile);
            Console.ReadKey();
        }
    }
}
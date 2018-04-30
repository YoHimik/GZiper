using System;
using GZiper.Core;

namespace GZiper {
    class Program {
        static void Main(string[] args) {
            string sourceFile = "test-img.jpg"; // исходный файл
            string compressedFile = "test-arch.gz"; // сжатый файл
            string newFile = "test-img-out.jpg";

            Ziper.Compress(sourceFile, compressedFile);
            Ziper.Decompress(compressedFile, newFile);
        }
    }
}
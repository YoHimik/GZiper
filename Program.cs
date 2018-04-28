using System;
using System.IO;
using System.IO.Compression;
using GZiper.Core;

namespace GZiper
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = "u12.jpg"; // исходный файл
            string compressedFile = "test2.gz"; // сжатый файл
            string newFile = "u13.jpg";


           Ziper.Compress(sourceFile, compressedFile);
         Ziper.Decompress(compressedFile, newFile);
            // Compress(sourceFile,compressedFile);
          
        }

        public static void Compress(string inDirection, string outDirection)
        {
            using (FileStream origStream = new FileStream(inDirection, FileMode.Open))
            {
                byte[] current = new byte[1024 * 1024];
                using (FileStream printStream = File.Create(outDirection))
                    //using (FileStream printStream = new FileStream())
                {
                    using (GZipStream compressStream = new GZipStream(printStream, CompressionMode.Compress))
                    {
                        int i = 0;
                        int k = 0;
                        while (i < origStream.Length)
                        {
                            origStream.Read(current, 0, current.Length);
                            compressStream.Write(current, 0, current.Length);
                            Console.WriteLine(current.Length);
                            i += current.Length;
                            k++;
                        }
                    }
                }
            }
        }
    }
}
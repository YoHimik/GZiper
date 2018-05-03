using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace GZiper.Core {
    public static class Ziper {
        public static void Compress(string inFile, string outFile) {
            Validation(inFile, outFile, true);
            var reader = new Reader(1024 * 1024);
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open)) {
                var threadReader = new Thread(delegate() { reader.Read(fileStream); });
                threadReader.Start();


                new Thread(delegate() { ArchiveThreadManager.Start(true); }).Start();

                using (var sfileStream = new FileStream(outFile, FileMode.CreateNew)) {
                    var writer = new Thread(delegate() { Writer.Write(sfileStream, true); });
                    writer.Start();

                    while (writer.IsAlive) {
                        Thread.Sleep(100);
                        WriteProgress(fileStream);
                        Console.Write("Done!");
                    }
                }
            }
        }

        private static void WriteProgress(FileStream fileStream) {
            Console.Clear();
            double k = (double) (fileStream.Position + 1) / fileStream.Length * 100;
            double p = k / (Reader.OrderId + 1);
            Console.WriteLine("{0}% reading done", Math.Round(k, 2));
            Console.WriteLine("{0}% writing done", Math.Round(p * Writer.OrderId, 2));
            Console.WriteLine("{0}% compressing done", Math.Round(p * ArchiveThreadManager.OrderId, 2));
            Console.WriteLine("{0}% total done",
                Math.Round((p * (ArchiveThreadManager.OrderId + Writer.OrderId) + k) / 3, 2));
        }

        public static void Decompress(string inFile, string outFile) {
            Validation(inFile, outFile, false);
            var reader = new Reader();
            using (var fileStream = new FileStream(inFile, FileMode.Open)) {
                var threadReader = new Thread(delegate() { reader.Read(fileStream); });
                threadReader.Start();


                new Thread(delegate() { ArchiveThreadManager.Start(false); }).Start();
                using (var sfileStream = new FileStream(outFile, FileMode.CreateNew)) {
                    var writer = new Thread(delegate() { Writer.Write(sfileStream, false); });
                    writer.Start();
                    while (writer.IsAlive) {
                        Thread.Sleep(100);
                        WriteProgress(fileStream);
                        Console.Write("Done!");
                    }
                }
            }
        }

        private static void Validation(string inFile, string outFile, bool isCompress) {
            if (!File.Exists(inFile))
                throw new FileNotFoundException("Incoming file not found!");
            var fileInfo = new FileInfo(inFile);
//            var fileIoPermission = new FileIOPermission(FileIOPermissionAccess.Read, fileInfo.FullName);
//            if ((fileIoPermission.AllFiles & FileIOPermissionAccess.Read) != 1)
//                throw new UnauthorizedAccessException("No access to read incoming file!");
            if (fileInfo.Extension.Equals(".gz") && isCompress)
                throw new ArgumentException("Incoming file already compressed!");
            if (!fileInfo.Extension.Equals(".gz") && !isCompress)
                throw new ArgumentException("Incoming file was not compressed!");
            if (File.Exists(outFile))
                throw new FileNotFoundException("Outgoing file already exists!");
//            fileIoPermission = new FileIOPermission(FileIOPermissionAccess.Write, fileInfo.Directory?.FullName);
//            if (fileIoPermission.IsUnrestricted())
//                throw new UnauthorizedAccessException("No access to write outgoing file!");
            fileInfo = new FileInfo(outFile);
            if (!fileInfo.Extension.Equals(".gz") && isCompress)
                throw new ArgumentException("Wrong outgoing file extension! Use \".gz\" .");
            if (fileInfo.Extension.Equals(".gz") && !isCompress)
                throw new ArgumentException("Wrong outgoing file extension!");
        }
    }
}
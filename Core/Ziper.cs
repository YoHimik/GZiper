using System;
using System.IO;
using System.Threading;

namespace GZiper.Core {
    public static class Ziper {
        private const ushort UpdateProgressTime = 100;
        private const ushort MinFileLength = 10;
        private const uint Mega = 1024 * 1024;

        public static ushort BlocksCount { get; private set; }
        private const string ArchiveExtension = ".gzz";


        public static void Compress(string inFile, string outFile) {
            Validate(inFile, outFile, true);
            Console.WriteLine("Compressing successfully started!");

            using (var writeStream = new FileStream(outFile, FileMode.Append)) {
                Writer.WriteKey(writeStream);
                Writer.WriteBlocksCount(writeStream, BlocksCount);
                var threadWriter = new Thread(delegate() { Writer.Write(writeStream, true); });
                using (var readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read)) {
                    var reader = new Reader(Mega);
                    var threadReader = new Thread(delegate() { reader.Read(readStream); });
                    threadReader.Start();
                    var archiveManagerThread = new Thread(delegate() { ArchiveThreadManager.Start(true); });
                    archiveManagerThread.Start();
                    threadWriter.Start();
                    while (threadReader.IsAlive)
                        WriteProgress();
                }

                while (threadWriter.IsAlive)
                    WriteProgress();
            }

            Console.WriteLine("Done!");
        }

        public static void Decompress(string inFile, string outFile) {
            Validate(inFile, outFile, false);
            Console.WriteLine("Decompressing successfully started!");

            using (var writeStream = new FileStream(outFile, FileMode.Append)) {
                var threadWriter = new Thread(delegate() { Writer.Write(writeStream, false); });
                using (var readStream = new FileStream(inFile, FileMode.Open)) {
                    var reader = new Reader();
                    BlocksCount = Reader.ReadBlocksCount(readStream);
                    var threadReader = new Thread(delegate() { reader.Read(readStream); });
                    threadReader.Start();
                    var archiveManagerThread = new Thread(delegate() { ArchiveThreadManager.Start(false); });
                    archiveManagerThread.Start();
                    threadWriter.Start();
                    while (threadReader.IsAlive)
                        WriteProgress();
                }

                while (threadWriter.IsAlive)
                    WriteProgress();
            }

            Console.WriteLine("Done!");
        }

        private static void WriteProgress() {
            Thread.Sleep(UpdateProgressTime);
            Console.CursorTop = 1;
            Console.CursorVisible = false;

            Console.WriteLine("{0}% reading done          ",
                Math.Round((double) 100 * Reader.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% compressing done      ",
                Math.Round((double) 100 * ArchiveThreadManager.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% writing done          ",
                Math.Round((double) 100 * Writer.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% total done            ",
                Math.Round(
                    (double) 100 * (ArchiveThreadManager.OrderId + Writer.OrderId + Reader.OrderId) / (3 * BlocksCount),
                    2));
        }

        private static void Validate(string inFile, string outFile, bool isCompress) {
            if (inFile.Equals(""))
                throw new ArgumentException("Wrong incoming file name!");
            if (!File.Exists(inFile))
                throw new FileNotFoundException("Incoming file not found!");
            var fileInfo = new FileInfo(inFile);
            if (fileInfo.Extension.Equals(ArchiveExtension) && isCompress)
                throw new ArgumentException("Incoming file already compressed!");
            if (!fileInfo.Extension.Equals(ArchiveExtension) && !isCompress)
                throw new ArgumentException("Incoming file was not compressed!");
            var currentDrive = new DriveInfo(Path.GetPathRoot(fileInfo.FullName));
            if (currentDrive.AvailableFreeSpace < fileInfo.Length / 100 * 105)
                throw new Exception("Not enough space for writing file.");
            if (isCompress)
                BlocksCount = (ushort) Math.Ceiling((double) fileInfo.Length / (1024 * 1024));
            if (outFile.Equals(""))
                throw new ArgumentException("Wrong outgoing file name!");
            if (File.Exists(outFile))
                throw new FileNotFoundException("Outgoing file already exists!");
            fileInfo = new FileInfo(outFile);
            if (!fileInfo.Extension.Equals(ArchiveExtension) && isCompress)
                throw new ArgumentException("Wrong outgoing file extension! Use \"" + ArchiveExtension + "\" .");
            if (fileInfo.Extension.Equals(ArchiveExtension) && !isCompress)
                throw new ArgumentException("Wrong outgoing file extension!");

            using (var readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read)) {
                if (!isCompress && (readStream.Length <= MinFileLength || !Reader.ReadAndCheckKey(readStream)))
                    throw new ArgumentException("Wrong incoming file format!");
            }

            using (var writeStream = new FileStream(outFile, FileMode.CreateNew)) {
            }
        }

        public static void Cancel() {
            Console.WriteLine("Program was canceled.");
            Environment.Exit(0);
        }
    }
}
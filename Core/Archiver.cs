using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZiper.Core {
    public class Archiver {
        private bool _isInterupeted;
        public bool Done { get; private set; }
        private const ushort SleepTime = 500;

        public Archiver() {
            _isInterupeted = false;
            Done = false;
        }

        public void Start(bool isCompress) {
            try {
                while (!_isInterupeted && ArchiveThreadManager.OrderId != Ziper.BlocksCount) {
                    var currentBlock = ArchiveThreadManager.DequeueBlock();
                    if (currentBlock == null) {
                        Thread.Sleep(SleepTime);
                        continue;
                    }

                    if (isCompress)
                        ComressBlock(currentBlock.Value);
                    else
                        DecompressBlock(currentBlock.Value);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                Ziper.Cancel();
            }

            Done = true;
        }

        private static void ComressBlock(Block block) {
            using (var memoryStream = new MemoryStream()) {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    gzipStream.Write(block.Bytes, 0, block.Bytes.Length);
                Writer.AddBlock(new Block(block.Number, memoryStream.ToArray()));
            }
        }

        private static void DecompressBlock(Block block) {
            using (var memoryStreamIn = new MemoryStream(block.Bytes)) {
                using (var zipStream = new GZipStream(memoryStreamIn, CompressionMode.Decompress)) {
                    using (var memoryStreamOut = new MemoryStream()) {
                        zipStream.CopyTo(memoryStreamOut);
                        Writer.AddBlock(new Block(block.Number, memoryStreamOut.ToArray()));
                    }
                }
            }
        }

        public void Interrup() {
            _isInterupeted = true;
        }
    }
}
using System;
using System.IO;
using System.Threading;

namespace GZiper.Core {
    public class Reader {
        private const ushort MaxAvaibleMemory = 512;
        private const ushort MaxBlockCount = 100;
        private const ushort BlockSizeLength = 3;
        private const ushort DecompressStartPosition = 4;
        private const ushort SleepTime = 500;
        private const ushort BlocksCountLength = 2;
        private const ushort Key = 0;
        private const ushort KeyLength = 4;

        private readonly uint _compressBlockSize;
        private bool _started;

        public static ushort OrderId { get; private set; }

        public Reader(uint compressBlockSize) {
            _compressBlockSize = compressBlockSize;
        }

        public Reader() {
        }

        public static bool ReadAndCheckKey(FileStream readStream) {
            var byteKey = new byte[KeyLength];
            readStream.Read(byteKey, 0, byteKey.Length);
            return BitConverter.ToUInt32(byteKey, 0) == Key;
        }

        public static ushort ReadBlocksCount(FileStream readStream) {
            readStream.Position = DecompressStartPosition;
            var blocksCount = new byte[BlocksCountLength];
            readStream.Read(blocksCount, 0, blocksCount.Length);
            return BitConverter.ToUInt16(blocksCount, 0);
        }


        public void Read(FileStream readStream) {
            if (_started)
                return;
            _started = true;
            try {
                while (readStream.Position < readStream.Length) {
                    if (!IsMemoryAvaible() || ArchiveThreadManager.GetBlockCount() > MaxBlockCount) {
                        Thread.Sleep(SleepTime);
                        continue;
                    }

                    var blockSize = !IsCompress() ? ReadBlockSize(readStream) : _compressBlockSize;
                    ReadBlock(readStream, blockSize);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                Ziper.Cancel();
            }
            _started = false;
        }

        private static void ReadBlock(Stream readStream, uint blockSize) {
            var sizeOfEnd = readStream.Length - readStream.Position;
            var block = sizeOfEnd < blockSize
                ? new byte[sizeOfEnd]
                : new byte[blockSize];
            readStream.Read(block, 0, block.Length);
            ArchiveThreadManager.EnqueueBlock(new Block(OrderId++, block));
        }

        private static uint ReadBlockSize(Stream readStream) {
            var blockSize = new byte[4];
            readStream.Read(blockSize, 0, BlockSizeLength);
            return BitConverter.ToUInt32(blockSize, 0);
        }

        private static bool IsMemoryAvaible() {
            return PcPerformance.Ram > MaxAvaibleMemory;
        }

        private bool IsCompress() {
            return _compressBlockSize != 0;
        }
    }
}
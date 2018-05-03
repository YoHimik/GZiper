using System;
using System.IO;


namespace GZiper.Core {
    public class Reader {
        private const ushort MaxAvaibleMemory = 512;
        private const ushort BlockSizeLength = 3;

        private readonly uint _compressBlockSize;
        private bool _started;
        
        public static ushort OrderId { get; private set; }

        public Reader(uint compressBlockSize) {
            _compressBlockSize = compressBlockSize;
        }

        public Reader() {
        }

        public void Read(FileStream fileStream) {
            if (_started)
                return;
            _started = true;
           
            while (fileStream.Position < fileStream.Length) {
                if (!IsMemoryAvaible() && PcPerformance.GetCpuValue() < 40)
                    continue;
                if (!IsCompress()) {
                    var blockSize = ReadBlockSize(fileStream);
                    ReadBlock(fileStream, blockSize);
                }
                else
                    ReadBlock(fileStream, _compressBlockSize);
            }

            Break();
        }

        private static bool IsMemoryAvaible() {
            return PcPerformance.GetRamValue() > MaxAvaibleMemory;
        }

        private bool IsCompress() {
            return _compressBlockSize != 0;
        }

        private static void ReadBlock(FileStream fileStream, uint blockSize) {
            var sizeOfEnd = fileStream.Length - fileStream.Position;
            var block = sizeOfEnd < blockSize
                ? new byte[sizeOfEnd]
                : new byte[blockSize];
            fileStream.Read(block, 0, block.Length);
            ArchiveThreadManager.EnqueueBlock(new Block(OrderId++, block));
        }

        private static uint ReadBlockSize(FileStream readStream) {
            var blockSize = new byte[4];
            readStream.Read(blockSize, 0, BlockSizeLength);
            return BitConverter.ToUInt32(blockSize, 0);
        }

        private static void Break() {
            ArchiveThreadManager.IsReadDone = true;
        }
    }
}
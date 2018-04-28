using System;
using System.Collections.Generic;
using System.IO;

namespace GZiper.Core
{
    public static class Reader
    {
        private const int Mega = 1024 * 1024;
        private const int MaxBlockCount = 2;
        private const int BlockSizeLength = 3;

        private static int _blockSize;
        private static bool _started;

        private static Queue<byte[]> _blocks;

        public static bool Done;

        public static void StartRead(string pathToFile, bool compress)
        {
            if (IsStarted())
                return;
            _blocks = new Queue<byte[]>();
            _blockSize = Mega;
            using (FileStream readStream = new FileStream(pathToFile, FileMode.Open))
            {
                while (readStream.Position < readStream.Length)
                {
                    if (GetCount() > MaxBlockCount)
                        continue;
                    if (!compress)
                    {
                        byte[] size = new byte[4];
                        readStream.Read(size, 0, BlockSizeLength);
                        _blockSize = BitConverter.ToInt32(size, 0);
                    }


                    byte[] block = readStream.Length - readStream.Position < _blockSize
                        ? new byte[readStream.Length - readStream.Position]
                        : new byte[_blockSize];
                    readStream.Read(block, 0, block.Length);
                    AddBlock(block);
                }
            }

            Break();
        }

        private static bool IsStarted()
        {
            if (_started)
                return true;
            _started = true;
            Done = false;
            return false;
        }

        private static void Break()
        {
            Done = true;
            _started = false;
        }

        public static void AddBlock(byte[] block)
        {
            lock (_blocks)
            {
                _blocks.Enqueue(block);
            }
        }

        public static byte[] GetBlock()
        {
            lock (_blocks)
            {
                return _blocks.Dequeue();
            }
        }

        public static int GetCount()
        {
            lock (_blocks)
            {
                return _blocks.Count;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace GZiper.Core
{
    public static class Writer
    {
        private static bool _started;
        public static bool Done;

        private static Queue<byte[]> _blocks;

        public static void StartWrite(string pathToFile, bool compress)
        {
            if (IsStarted())
                return;
            _blocks = new Queue<byte[]>();
            using (FileStream stream = new FileStream( pathToFile, FileMode.Append))
            {
                while (!Collector.Done || GetCount() > 0)
                {
                    if (GetCount() == 0)
                        continue;
                    byte[] block = GetBlock();
                    if ( compress)
                        stream.Write(BitConverter.GetBytes(block.Length), 0, 3);
                    stream.Write(block, 0, block.Length);
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
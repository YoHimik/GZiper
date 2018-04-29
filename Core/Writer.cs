using System;
using System.Collections.Generic;
using System.IO;

namespace GZiper.Core {
    public static class Writer {
        private static bool _started;
        public static bool Done;

        private static List<Block> _blocks;

        public static void addBlock() {
            _blocks = new List<Block>();
        }

        public static void StartWrite(string pathToFile, bool compress) {
            if (IsStarted())
                return;
            int number = 1;
            using (FileStream writeStream = new FileStream(pathToFile, FileMode.Append)) {
                while (!IsCollectorsDone() || GetCount() > 0) {
                    if (GetCount() == 0)
                        continue;
                    Block block = GetBlock(number);
                    if (block.Number != number)
                        continue;
                    number++;
                    if (compress)
                        WriteBlockSize(writeStream, block.Bytes.Length);
                    writeStream.Write(block.Bytes, 0, block.Bytes.Length);
                    Console.WriteLine(block.Bytes.Length + " o");

                }
            }

            Break();
        }

        private static bool IsCollectorsDone() {
            bool done = true;
            foreach (Collector collector in Ziper.Collectors)
                done &= collector.Done;
            return done;
        }

        private static void WriteBlockSize(FileStream writeStream, int size) {
            writeStream.Write(BitConverter.GetBytes(size), 0, 3);
        }

        private static bool IsStarted() {
            if (_started)
                return true;
            _started = true;
            Done = false;
            return false;
        }

        private static void Break() {
            Done = true;
            _started = false;
        }

        public static void AddBlock(Block block) {
            lock (_blocks) {
                _blocks.Add(block);
            }
        }

        public static Block GetBlock(int number) {
            lock (_blocks) {
                Block b = new Block();
                for (int i = 0; i < _blocks.Count; i++)
                    if (_blocks[i].Number == number) {
                        b = _blocks[i];
                        _blocks.RemoveAt(i);
                    }

                return b;
            }
        }

        public static int GetCount() {
            lock (_blocks) {
                return _blocks.Count;
            }
        }
    }
}
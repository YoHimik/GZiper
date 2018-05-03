using System;
using System.Collections.Generic;
using System.IO;


namespace GZiper.Core {
    public static class Writer {
        private static bool _started;
        public static bool IsCompressDone { private get; set; }
        public static ushort OrderId { get; private set; }

        private static readonly List<Block> Blocks;

        static Writer() {
            Blocks = new List<Block>();
            OrderId = 0;
        }

        public static void Write(FileStream writeStream, bool compress) {
            if (_started)
                return;
            _started = true;
            IsCompressDone = false;

            
            while (!IsCompressDone || GetCount() > 0) 
                WriteBlock( compress, writeStream);
            Break();
        }

        private static void WriteBlockSize(FileStream writeStream, int size) {
            writeStream.Write(BitConverter.GetBytes(size), 0, 3);
        }

        private static void WriteBlock( bool compress, FileStream writeStream) {
            var block = GetBlock(OrderId);
            if (block == null)
                return;
            OrderId++;
            if (compress)
                WriteBlockSize(writeStream, block.Value.Bytes.Length);
            writeStream.Write(block.Value.Bytes, 0, block.Value.Bytes.Length);
        }


        private static void Break() {
            _started = false;
        }

        public static void AddBlock(Block block) {
            lock (Blocks) {
                Blocks.Add(block);
            }
        }

        private static Block? GetBlock(uint number) {
            lock (Blocks) {
                for (var i = 0; i < Blocks.Count; i++)
                    if (Blocks[i].Number == number) {
                        var b = Blocks[i];
                        Blocks.RemoveAt(i);
                        return b;
                    }

                return null;
            }
        }

        private static int GetCount() {
            lock (Blocks) {
                return Blocks.Count;
            }
        }
    }
}
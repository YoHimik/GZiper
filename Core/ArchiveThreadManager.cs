using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GZiper.Core {
    public static class ArchiveThreadManager {
        private const ushort MaxCpuValue = 80;
        public static bool IsReadDone { get; set; }
        private static readonly Queue<Block> BlockQueue = new Queue<Block>();
        private static readonly Queue<Archiver> Archivers = new Queue<Archiver>();
        public static ushort OrderId { get; private set; }

        public static void Start(bool isCompress) {
            OrderId = 0;
            while (!IsReadDone || GetBlockCount() > 0 || !IsProcessDone()) {
                Thread.Sleep(500);
               

                if ( PcPerformance.GetCpuValue() < MaxCpuValue) {
                    var archiver = new Archiver();
                    Archivers.Enqueue(archiver);

                    if (isCompress)
                        new Thread(Archivers.Peek().Compress).Start();
                    else
                        new Thread(Archivers.Peek().Decompress).Start();
                }

                if (Archivers.Count > 1 && (PcPerformance.GetCpuValue() >= MaxCpuValue || GetBlockCount() < Get()*10))
                    Archivers.Dequeue().Interrup();

//                Console.Clear();
//                Console.WriteLine(PcPerformance.GetCpuValue() + " CPU");
//                Console.WriteLine(PcPerformance.GetRamValue() + " RAM");
//                Console.WriteLine(GetBlockCount() + " b");
//                Console.WriteLine(Archivers.Count + " a");
               
            }

            Writer.IsCompressDone = true;
        }

        public static int Get() {
            return Archivers.Count;
        }

        private static bool IsProcessDone() {
            if (Archivers.Count == 0) return true;
            foreach (var archiver in Archivers)
                if (!archiver.IsDone)
                    return false;

            return true;
        }


        public static int GetBlockCount() {
            lock (BlockQueue) {
                return BlockQueue.Count;
            }
        }

        public static void EnqueueBlock(Block block) {
            lock (BlockQueue) {
                BlockQueue.Enqueue(block);
            }
        }

        public static Block? DequeueBlock() {
            lock (BlockQueue) {
                if (BlockQueue.Count > 0) {
                    OrderId++;
                    return BlockQueue.Dequeue();
                }

                return null;
            }
        }
    }
}
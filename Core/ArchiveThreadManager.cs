using System;
using System.Collections.Generic;
using System.Threading;

namespace GZiper.Core {
    public static class ArchiveThreadManager {
        private const ushort MaxCpuValue = 60;
        private const ushort BlocksForThread = 10;
        private const ushort SleepTime = 1000;

        public static ushort OrderId { get; private set; }

        private static readonly Queue<Block> BlockQueue = new Queue<Block>();
        private static readonly Stack<Archiver> Archivers = new Stack<Archiver>();

        public static void Start(bool isCompress) {
            try {
                var MaxThreads = Environment.ProcessorCount;
                OrderId = 0;
                AddArchiver(isCompress);
                while (OrderId != Ziper.BlocksCount) {
                    if (Archivers.Count * BlocksForThread < BlockQueue.Count && Archivers.Count < MaxThreads &&
                        PcPerformance.Cpu < MaxCpuValue)
                        AddArchiver(isCompress);
                    else
                        RemoveArchiver();
                    Thread.Sleep(SleepTime);
                }

                WaitThreads();
            }
            catch (Exception e) {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                Ziper.Cancel();
            }
        }

        private static void WaitThreads() {
            while (Archivers.Count > 0)
                if (Archivers.Peek().Done)
                    Archivers.Pop();
        }


        public static int GetBlockCount() {
            return BlockQueue.Count;
        }

        private static void AddArchiver(bool isCompress) {
            var archiver = new Archiver();
            Archivers.Push(archiver);
            if (isCompress)
                new Thread(delegate() { Archivers.Peek().Start(true); }).Start();
            else
                new Thread(delegate() { Archivers.Peek().Start(false); }).Start();
        }

        private static void RemoveArchiver() {
            if (Archivers.Count < 2)
                return;
            var archiver = Archivers.Pop();
            archiver.Interrup();
        }

        public static void EnqueueBlock(Block block) {
            lock (BlockQueue) {
                BlockQueue.Enqueue(block);
            }
        }

        public static Block? DequeueBlock() {
            lock (BlockQueue) {
                if (BlockQueue.Count == 0) return null;
                OrderId++;
                return BlockQueue.Dequeue();
            }
        }
    }
}
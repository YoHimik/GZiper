using System.Threading;

namespace GZiper.Core {
    public static class Ziper {
        public static Collector[] Collectors;
        public static Thread[] CollectorsThreads;
        private const int ThreadCount = 1;

        public static void Compress(string inFile, string outFile) {
            Thread reader = new Thread(delegate() { Reader.StartRead(inFile, true); });
            Thread writer = new Thread(delegate() { Writer.StartWrite(outFile, true); });
            Writer.addBlock();
            Reader.addBlock();
            Collectors = new Collector[ThreadCount];
            CollectorsThreads = new Thread[ThreadCount];
            reader.Start();
            for (int i = 0; i < ThreadCount; i++) {
                Collectors[i] = new Collector();
                CollectorsThreads[i] = new Thread(Collectors[i].Compress);
                CollectorsThreads[i].Start();
            }

            writer.Start();
            writer.Join();
        }

        public static void Decompress(string inFile, string outFile) {
            Thread reader = new Thread(delegate() { Reader.StartRead(inFile, false); });
            Thread writer = new Thread(delegate() { Writer.StartWrite(outFile, false); });
            Writer.addBlock();
            Reader.addBlock();
            Collectors = new Collector[ThreadCount];
            CollectorsThreads = new Thread[ThreadCount];
            reader.Start();
            for (int i = 0; i < ThreadCount; i++) {
                Collectors[i] = new Collector();
                CollectorsThreads[i] = new Thread(Collectors[i].Decompress);
                CollectorsThreads[i].Start();
            }

            writer.Start();
            writer.Join();
        }
    }
}
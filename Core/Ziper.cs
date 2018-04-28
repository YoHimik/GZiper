using System.Threading;

namespace GZiper.Core
{
    public static class Ziper
    {
        public static void Compress(string inFile, string outFile)
        {
            Thread reader = new Thread(delegate() { Reader.StartRead(inFile, true); });
            Thread writer = new Thread(delegate () { Writer.StartWrite(outFile, true); });
            Thread collector = new Thread(Collector.Compress);

            reader.Start();
            collector.Start();
            writer.Start();
            writer.Join();
        }

        public static void Decompress(string inFile, string outFile)
        {
            Thread reader = new Thread(delegate () { Reader.StartRead(inFile, false); });
            Thread writer = new Thread(delegate () { Writer.StartWrite(outFile, false); });
            Thread collector = new Thread(Collector.Decompress);

            reader.Start();
            collector.Start();
            writer.Start();
            writer.Join();
        }
    }
}
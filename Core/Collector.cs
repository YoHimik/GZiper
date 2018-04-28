using System.IO;
using System.IO.Compression;

namespace GZiper.Core
{
    public static class Collector
    {
        public static bool Done;

        public static void Compress()
        {
            Done = false;

            while (!Reader.Done || Reader.GetCount() > 0)
            {
                if (Reader.GetCount() == 0)
                    continue;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        byte[] b = Reader.GetBlock();

                        zipStream.Write(b, 0, b.Length);
                        byte[] r = memoryStream.ToArray();
                        Writer.AddBlock(r);
                    }
                }
            }

            Done = true;
        }


        public static void Decompress()
        {
            Done = false;
            while (!Reader.Done || Reader.GetCount() > 0)
            {
                if (Reader.GetCount() == 0)
                    continue;
                byte[] b = Reader.GetBlock();
                using (MemoryStream memoryStream = new MemoryStream(b))
                {
                    using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        byte[] r = new byte[1024 * 1024];
                        zipStream.Read(r, 0, r.Length);

                        Writer.AddBlock(r);
                    }
                }
            }

            Done = true;
        }
    }
}
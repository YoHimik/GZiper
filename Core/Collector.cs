using System.IO;
using System.IO.Compression;

namespace GZiper.Core {
    public class Collector {
        public bool Done;

        public Collector() {
            Done = false;
        }

        public void Compress() {
            while (!Reader.Done || Reader.GetCount() > 0) {
                Block b = Reader.TryGetBlock();
                if (b.Number <= 0)
                    continue;

                using (MemoryStream memoryStream = new MemoryStream()) {
                    using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress)) {
                        /*using (MemoryStream m = new MemoryStream(b.Bytes)) {
                            m.CopyTo(zipStream);
                            r = memoryStream.ToArray();
                       }*/
                        zipStream.Write(b.Bytes, 0, b.Bytes.Length);
                    }

                    byte[] r = memoryStream.ToArray();
                    Writer.AddBlock(new Block(b.Number, r));
                }
            }

            Done = true;
        }


        public void Decompress() {
            while (!Reader.Done || Reader.GetCount() > 0) {
                Block b = Reader.TryGetBlock();
                if (b.Number <= 0)
                    continue;
                using (MemoryStream memoryStream = new MemoryStream(b.Bytes)) {
                    using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Decompress)) {
                        /*  using (MemoryStream m = new MemoryStream()) {
                              zipStream.CopyTo(m);
                              r = m.ToArray();
                          }
                          Writer.AddBlock(new Block(b.Number, r));*/
                        byte[] r = new byte[1024 * 1024];
                        zipStream.Read(r, 0, r.Length);

                        Writer.AddBlock(new Block(b.Number, r));
                    }
                }
            }

            Done = true;
        }
    }
}
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
                        zipStream.Write(b.Bytes, 0, b.Bytes.Length);
                    }

                    b.Bytes = memoryStream.ToArray();
                    
                    Writer.AddBlock(b);
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
                        using (MemoryStream m = new MemoryStream()) {
                            zipStream.CopyTo(m);
                            b.Bytes = m.ToArray();
                        }

                      
                        Writer.AddBlock(b);
                    }
                }
            }
            Done = true;
        }
    }
}
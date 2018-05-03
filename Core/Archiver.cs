using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZiper.Core {
    public class Archiver  {
        private bool _isInterupeted;
        public bool IsDone { get; private set; }

        
      
        public Archiver() {
            _isInterupeted = false;
        }

        public void Compress() {
            while (!_isInterupeted && (ArchiveThreadManager.GetBlockCount() > 0 || !ArchiveThreadManager.IsReadDone)) {
                ComressBlock();
            }

            IsDone = true;
        }

        private static void ComressBlock() {
            var currentBlock = ArchiveThreadManager.DequeueBlock();
            if (currentBlock == null) {
                return;
            }

            using (var memoryStream = new MemoryStream()) {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress)) {
                    gzipStream.Write(currentBlock.Value.Bytes, 0, currentBlock.Value.Bytes.Length);
                }

                Writer.AddBlock(new Block(currentBlock.Value.Number, memoryStream.ToArray()));
            }
        }

        public void Decompress() {
            while (!_isInterupeted && (ArchiveThreadManager.GetBlockCount() > 0 || !ArchiveThreadManager.IsReadDone))
                DecomressBlock();
            IsDone = true;
        }

        private static void DecomressBlock() {
            var currentBlock = ArchiveThreadManager.DequeueBlock();
            if (currentBlock == null)
                return;
            using (var memoryStream = new MemoryStream(currentBlock.Value.Bytes)) {
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Decompress)) {
                    using (var memoryStreamOut = new MemoryStream()) {
                        zipStream.CopyTo(memoryStreamOut);
                        Writer.AddBlock(new Block(currentBlock.Value.Number, memoryStreamOut.ToArray()));
                    }
                }
            }
        }

        public void Interrup() {
            _isInterupeted = true;
        }
    }
}
namespace GZiper.Core {
    public struct Block {
        public ushort Number { get; }
        public byte[] Bytes { get; }

        public Block(ushort number, byte[] bytes) {
            Number = number;
            Bytes = bytes;
        }
    }
}
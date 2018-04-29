namespace GZiper.Core {
    public struct Block {
        public int Number;
        public byte[] Bytes;

        public Block(int number, byte[] bytes) {
            Number = number;
            Bytes = bytes;
        }

        public Block(int number = -1) {
            Number = number;
            Bytes = null;
        }
    }
}
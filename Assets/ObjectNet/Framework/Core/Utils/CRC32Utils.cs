namespace com.onlineobject.objectnet {
    public class CRC32Utils {
        private static readonly uint[] Table;

        static CRC32Utils() {
            Table = new uint[256];
            const uint polynomial = 0xedb88320;
            for (uint i = 0; i < 256; i++) {
                uint crc = i;
                for (uint j = 8; j > 0; j--) {
                    if ((crc & 1) == 1) {
                        crc = (crc >> 1) ^ polynomial;
                    } else {
                        crc >>= 1;
                    }
                }
                Table[i] = crc;
            }
        }

        public static uint ComputeHash(string input) {
            uint crc = 0xffffffff;
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(input)) {
                byte tableIndex = (byte)(((crc) & 0xff) ^ b);
                crc = (crc >> 8) ^ Table[tableIndex];
            }
            return ~crc;
        }
        public static ulong CombineCRC32(uint crc1, uint crc2) {
            // Shift the first CRC to the upper 32 bits and combine with the second CRC
            return ((ulong)crc1 << 32) | crc2;
        }
    }
}
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Helper class for ShortFloat conversions
    /// </summary>
    public static class ShortFloatHelper {
        private static uint[]   mantissaTable   = GenerateMantissaTable();
        private static uint[]   exponentTable   = GenerateExponentTable();
        private static ushort[] offsetTable     = GenerateOffsetTable();
        private static ushort[] baseTable       = GenerateBaseTable();
        private static sbyte[]  shiftTable      = GenerateShiftTable();

        [StructLayout(LayoutKind.Explicit)]
        struct UIntFloat {
            [FieldOffset(0)]
            public uint UIntValue;

            [FieldOffset(0)]
            public float FloatValue;
        }

        static UIntFloat floatToIntConverter = new UIntFloat { FloatValue = 0 };

        static uint FloatToUInt(float v)
        {
            floatToIntConverter.FloatValue = v;
            return floatToIntConverter.UIntValue;
        }

        static float UIntToFloat(uint v) {
            floatToIntConverter.UIntValue = v;
            return floatToIntConverter.FloatValue;
        }
        
        private static uint ConvertMantissa(int i) {
            uint m = (uint)(i << 13);
            uint e = 0;

            while ((m & 0x00800000) == 0) {
                e -= 0x00800000;
                m <<= 1;
            }
            m &= unchecked((uint)~0x00800000);
            e += 0x38800000;
            return m | e;
        }

        private static uint[] GenerateMantissaTable() {
            uint[] mantissaTable = new uint[2048];
            mantissaTable[0] = 0;
            for (int i = 1; i < 1024; i++) {
                mantissaTable[i] = ConvertMantissa(i);
            }
            for (int i = 1024; i < 2048; i++) {
                mantissaTable[i] = (uint)(0x38000000 + ((i - 1024) << 13));
            }

            return mantissaTable;
        }
        private static uint[] GenerateExponentTable() {
            uint[] exponentTable = new uint[64];
            exponentTable[0] = 0;
            for (int i = 1; i < 31; i++) {
                exponentTable[i] = (uint)(i << 23);
            }
            exponentTable[31] = 0x47800000;
            exponentTable[32] = 0x80000000;
            for (int i = 33; i < 63; i++) {
                exponentTable[i] = (uint)(0x80000000 + ((i - 32) << 23));
            }
            exponentTable[63] = 0xc7800000;

            return exponentTable;
        }
        private static ushort[] GenerateOffsetTable() {
            ushort[] offsetTable = new ushort[64];
            
            offsetTable[0] = 0;
            for (int i = 1; i < 32; i++) {
                offsetTable[i] = 1024;
            }

            offsetTable[32] = 0;
            for (int i = 33; i < 64; i++) {
                offsetTable[i] = 1024;
            }

            return offsetTable;
        }

        private static ushort[] GenerateBaseTable() {
            ushort[] baseTable = new ushort[512];

            for (int i = 0; i < 256; ++i) {
                sbyte e = (sbyte)(127 - i);
                if (e > 24) {
                    baseTable[i | 0x000] = 0x0000;
                    baseTable[i | 0x100] = 0x8000;
                } else if (e > 14) {
                    baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
                    baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
                } else if (e >= -15) {
                    baseTable[i | 0x000] = (ushort)((15 - e) << 10);
                    baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
                } else if (e > -128) {
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                } else {
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
            }

            return baseTable;
        }
        private static sbyte[] GenerateShiftTable()        {
            sbyte[] shiftTable = new sbyte[512];

            for (int i = 0; i < 256; ++i) {
                sbyte e = (sbyte)(127 - i);
                if (e > 24) {
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                } else if (e > 14) {
                    shiftTable[i | 0x000] = (sbyte)(e - 1);
                    shiftTable[i | 0x100] = (sbyte)(e - 1);
                } else if (e >= -15) {
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                } else if (e > -128) {
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                } else {
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
            }

            return shiftTable;
        }
                
        public static float HalfToSingle(ShortFloat half) {
            uint result = mantissaTable[offsetTable[half.internalValue >> 10] + (half.internalValue & 0x3ff)] + exponentTable[half.internalValue >> 10];

            return UIntToFloat(result);
        }

        public static ShortFloat SingleToHalf(float single) {
            uint value = FloatToUInt(single);
            ushort result = (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
            return ShortFloat.ToHalf(result);
        }
        
        public static float Decompress(ushort compressedFloat) {
            uint result = mantissaTable[offsetTable[compressedFloat >> 10] + (compressedFloat & 0x3ff)] + exponentTable[compressedFloat >> 10];
            return UIntToFloat(result);
        }

        public static ushort Compress(float uncompressedFloat) {
            uint value = FloatToUInt(uncompressedFloat);
            return (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
        }
        
        public static ShortFloat Negate(ShortFloat half) {
            return ShortFloat.ToHalf((ushort)(half.internalValue ^ 0x8000));
        }

        public static ShortFloat Abs(ShortFloat half) {
            return ShortFloat.ToHalf((ushort)(half.internalValue & 0x7fff));
        }

        public static bool IsNaN(ShortFloat half) {
            return ((half.internalValue & 0x7fff) > 0x7c00);
        }

        public static bool IsInfinity(ShortFloat half) {
            return ((half.internalValue & 0x7fff) == 0x7c00);
        }

        public static bool IsPositiveInfinity(ShortFloat half) {
            return (half.internalValue == 0x7c00);
        }

        public static bool IsNegativeInfinity(ShortFloat half) {
            return (half.internalValue == 0xfc00);
        }

    }
}

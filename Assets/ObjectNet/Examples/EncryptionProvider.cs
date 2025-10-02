using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class EncryptionProvider : MonoBehaviour, IInformationProvider {

        readonly byte[] key    = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        readonly byte[] iv     = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        
        public byte[] Encrypt(byte[] data) {
            return this.Encrypt(data, key, iv);
        }

        public byte[] Decrypt(byte[] data) {
            return this.Decrypt(data, key, iv);
        }

        private byte[] Encrypt(byte[] data, byte[] key, byte[] iv) {
            using (var aes = Aes.Create()) {
                aes.KeySize     = 128;
                aes.BlockSize   = 128;
                aes.Padding     = PaddingMode.Zeros;

                aes.Key         = key;
                aes.IV          = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    return PerformCryptography(data, encryptor, CryptoStreamMode.Write);
                }
            }
        }

        private byte[] Decrypt(byte[] data, byte[] key, byte[] iv) {
            using (var aes = Aes.Create()) {
                aes.KeySize     = 128;
                aes.BlockSize   = 128;
                aes.Padding     = PaddingMode.Zeros;

                aes.Key         = key;
                aes.IV          = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                    return PerformCryptography(data, decryptor, CryptoStreamMode.Read);
                }
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform, CryptoStreamMode mode) {
            using (var ms = ((CryptoStreamMode.Read.Equals(mode)) ? new MemoryStream(data) : new MemoryStream())) {
                using (var cryptoStream = new CryptoStream(ms, cryptoTransform, mode)) {
                    if (CryptoStreamMode.Write.Equals(mode)) {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    } else if (CryptoStreamMode.Read.Equals(mode)) {
                        cryptoStream.Read(data, 0, data.Length);
                    }
                    byte[] result = ms.ToArray();
                    return result;
                }
            }
        }
    }
}
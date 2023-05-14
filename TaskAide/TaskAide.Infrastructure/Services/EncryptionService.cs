using System.Security.Cryptography;
using TaskAide.Domain.Services;

namespace TaskAide.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(byte[] key)
        {
            _key = key;
        }

        public string EncryptString(string plainString)
        {
            ArgumentNullException.ThrowIfNull(plainString, nameof(plainString));

            byte[] encrypted = Encrypt(plainString, _key);

            var encryptedString = Convert.ToBase64String(encrypted);
            return encryptedString;
        }

        public string DecryptString(string encryptedString)
        {
            ArgumentNullException.ThrowIfNull(encryptedString, nameof(encryptedString));

            string decrypted = Decrypt(encryptedString, _key);
            return decrypted;
        }

        private static byte[] Encrypt(string plainText, byte[] key)
        {
            byte[] encrypteString;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                byte[] IV = aes.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypteString = msEncrypt.ToArray();
                    }
                }

                byte[] cipherTextBytes = new byte[IV.Length + encrypteString.Length];
                Buffer.BlockCopy(IV, 0, cipherTextBytes, 0, IV.Length);
                Buffer.BlockCopy(encrypteString, 0, cipherTextBytes, IV.Length, encrypteString.Length);
 
                return cipherTextBytes;
            }
        }

        private static string Decrypt(string cipherText, byte[] Key)
        {
            string? plainText = null;
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            byte[] IV = cipherTextBytes.Take(16).ToArray();
            byte[] encryptedString = cipherTextBytes.Skip(16).ToArray();

            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream(encryptedString))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                            plainText = reader.ReadToEnd();
                    }
                }
            }

            return plainText;
        }
    }
}

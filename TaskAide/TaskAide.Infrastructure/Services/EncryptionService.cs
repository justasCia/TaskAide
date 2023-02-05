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
            byte[] encrypted = Encrypt(plainString, _key);
            // Convert bytes to string
            var encryptedString = Convert.ToBase64String(encrypted);
            return encryptedString;
        }

        public string DecryptString(string encryptedString)
        {
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

                // Create the streams used for encryption.
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

                // Concating two arrays 'iv' and 'encryptedPlainText' because the encrypted string needs to have
                // IV and encrypted string value in it. That's the only possible way to decrypt an encrypted connection string
                byte[] cipherTextBytes = new byte[IV.Length + encrypteString.Length];
                Buffer.BlockCopy(IV, 0, cipherTextBytes, 0, IV.Length);
                Buffer.BlockCopy(encrypteString, 0, cipherTextBytes, IV.Length, encrypteString.Length);

                // Return encrypted string  
                return cipherTextBytes;
            }
        }

        private static string Decrypt(string cipherText, byte[] Key)
        {
            string? plainText = null;
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            // Extracting IV and connection string from ciphertext
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
            // Return decrypted string
            return plainText;
        }
    }
}

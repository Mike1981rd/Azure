using System.Security.Cryptography;
using System.Text;

namespace WebsiteBuilderAPI.Services.Encryption
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _encryptionKey;
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
        {
            _encryptionKey = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
            _logger = logger;

            // Verificar que la clave tenga al menos 32 caracteres para AES-256
            if (_encryptionKey.Length < 32)
            {
                throw new InvalidOperationException("Encryption key must be at least 32 characters long");
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            try
            {
                using (var aes = Aes.Create())
                {
                    // Usar los primeros 32 bytes de la clave como key para AES-256
                    var key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
                    aes.Key = key;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Escribir IV al inicio del stream
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        var encrypted = msEncrypt.ToArray();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new InvalidOperationException("Encryption failed", ex);
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            try
            {
                var encrypted = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    // Usar los primeros 32 bytes de la clave como key para AES-256
                    var key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
                    aes.Key = key;

                    // Extraer IV del inicio del array
                    var iv = new byte[aes.IV.Length];
                    Array.Copy(encrypted, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    // Datos encriptados sin el IV
                    var encryptedData = new byte[encrypted.Length - iv.Length];
                    Array.Copy(encrypted, iv.Length, encryptedData, 0, encryptedData.Length);

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(encryptedData))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                throw new InvalidOperationException("Decryption failed", ex);
            }
        }
    }
}
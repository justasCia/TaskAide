namespace TaskAide.Domain.Services
{
    public interface IEncryptionService
    {
        string EncryptString(string plainString);
        string DecryptString(string encryptedString);
    }
}

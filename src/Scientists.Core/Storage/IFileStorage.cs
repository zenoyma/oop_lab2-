namespace Scientists.Core.Storage;

// Де лежать файли: на диску (name - повний шлях) або на Google Drive (name - назва файлу)
public interface IFileStorage
{
    Task SaveAsync(string name, byte[] content, string mimeType);

    Task<byte[]> LoadAsync(string name);
}

// Сховище, яке може показати список своїх файлів (Google Drive).
// Локальні файли обирають через FilePicker, тому їм список не потрібен
public interface IListableStorage
{
    Task<List<string>> ListAsync(string extension);
}

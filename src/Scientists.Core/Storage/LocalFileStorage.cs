namespace Scientists.Core.Storage;

public class LocalFileStorage : IFileStorage
{
    public Task SaveAsync(string name, byte[] content, string mimeType)
        => File.WriteAllBytesAsync(name, content);

    public Task<byte[]> LoadAsync(string name)
        => File.ReadAllBytesAsync(name);
}

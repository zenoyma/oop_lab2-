namespace Scientists.Core.Saving;

// Продукт Фабричного методу: об'єкт, який записує результати у вже відкритий потік
public interface IResultWriter
{
    string Extension { get; }

    string MimeType { get; }

    void Write(Stream output, IReadOnlyList<Scientist> results);
}

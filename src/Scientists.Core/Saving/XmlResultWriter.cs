namespace Scientists.Core.Saving;

// Відфільтровані записи у тому ж XML-форматі, що й вхідний файл
public class XmlResultWriter : IResultWriter
{
    public string Extension => ".xml";

    public string MimeType => "application/xml";

    public void Write(Stream output, IReadOnlyList<Scientist> results)
        => Scientist.ToDocument(results).Save(output);
}

using Scientists.Core.Storage;

namespace Scientists.Core.Saving;

// Фабричний метод: базовий клас відкриває "файл" і зберігає його в сховище,
// а який об'єкт пише в цей файл (XML чи HTML) - вирішує підклас у CreateWriter
public abstract class ResultSaver
{
    protected abstract IResultWriter CreateWriter();

    // Повертає назву, під якою файл збережено (з потрібним розширенням)
    public async Task<string> SaveAsync(IFileStorage storage, string name, IReadOnlyList<Scientist> results)
    {
        IResultWriter writer = CreateWriter();
        if (!name.EndsWith(writer.Extension, StringComparison.OrdinalIgnoreCase))
        {
            name += writer.Extension;
        }

        using var file = new MemoryStream();   // файл спочатку пишемо в пам'ять, бо Drive приймає готовий вміст
        writer.Write(file, results);
        await storage.SaveAsync(name, file.ToArray(), writer.MimeType);
        return name;
    }
}

public class XmlResultSaver : ResultSaver
{
    protected override IResultWriter CreateWriter() => new XmlResultWriter();
}

public class HtmlResultSaver : ResultSaver
{
    private readonly string xsl;

    public HtmlResultSaver(string xsl)
    {
        this.xsl = xsl;
    }

    protected override IResultWriter CreateWriter() => new HtmlResultWriter(xsl);
}

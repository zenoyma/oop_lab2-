using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Scientists.Core.Logging;
using Scientists.Core.Saving;
using Scientists.Core.Search;
using Scientists.Core.Storage;

namespace Scientists.Core;

public enum Place
{
    File,
    Drive,
}

// Посередник між вікном і логікою: вікно лише викликає ці методи і показує результат
public class ScientistsController
{
    private readonly IFileStorage fileStorage;
    private readonly IFileStorage? driveStorage;
    private string? xml;
    private string? xsl;
    private List<Scientist>? results;   // null - пошук ще не виконували

    // Три способи аналізу; вікно обирає спосіб за номером у цьому масиві
    private readonly IXmlSearchStrategy[] strategies =
    {
        new SaxSearchStrategy(),
        new DomSearchStrategy(),
        new LinqSearchStrategy(),
    };

    // logPath - файл журналу подій; null - журнал не ведеться (так у тестах)
    public ScientistsController(IFileStorage fileStorage, IFileStorage? driveStorage = null, string? logPath = null)
    {
        this.fileStorage = fileStorage;
        this.driveStorage = driveStorage;
        if (logPath != null)
        {
            EventLog.Instance.FilePath = logPath;
        }
    }

    public string? XmlName { get; private set; }

    public string? XslName { get; private set; }

    public IReadOnlyList<Scientist> Results => results ?? new List<Scientist>();

    public async Task OpenXmlAsync(Place place, string name)
    {
        string text = await LoadTextAsync(place, name);
        XDocument.Parse(text);   // одразу перевіряємо, що це правильний XML (інакше XmlException)
        xml = text;
        XmlName = Path.GetFileName(name);
        results = null;
    }

    public async Task OpenXslAsync(Place place, string name)
    {
        string text = await LoadTextAsync(place, name);
        using (var reader = XmlReader.Create(new StringReader(text)))
        {
            new XslCompiledTransform().Load(reader);   // перевірка XSL (інакше XsltException)
        }
        xsl = text;
        XslName = Path.GetFileName(name);
    }

    // Значення атрибута, які є у відкритому файлі - для списків вибору
    public List<string> GetAttributeValues(string attribute)
    {
        if (xml == null) return new List<string>();
        return XDocument.Parse(xml)
            .Descendants(Scientist.ElementName)
            .Select(e => (string?)e.Attribute(attribute))
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => v!)
            .Distinct()
            .Order()
            .ToList();
    }

    public List<string> GetStrategyNames() => strategies.Select(s => s.Name).ToList();

    // Атрибути для списків вибору у вікні: назва в XML -> підпис
    public IReadOnlyDictionary<string, string> GetSearchAttributes() => Scientist.SearchAttributes;

    // selected: атрибут -> обране значення (null - будь-яке)
    public IReadOnlyList<Scientist> Search(int strategyIndex, IReadOnlyDictionary<string, string?> selected)
    {
        string source = xml ?? throw new InvalidOperationException("Спочатку відкрийте XML-файл");
        IXmlSearchStrategy strategy = strategies[strategyIndex];
        var criteria = new SearchCriteria();
        foreach (var (attribute, value) in selected)
        {
            criteria.With(attribute, value);
        }
        results = strategy.Search(source, criteria);
        EventLog.Instance.Write(EventLevel.Filter,
            $"Спосіб: {strategy.Name}. Параметри: {criteria}. Знайдено: {results.Count}");
        return results;
    }

    public void Clear() => results = null;

    // Фабричний метод: контролер обирає потрібний ResultSaver, а як писати файл - вирішує він сам
    public async Task<string> SaveResultsAsync(Place place, string name, bool asHtml)
    {
        List<Scientist> found = results ?? throw new InvalidOperationException("Спочатку виконайте пошук");
        ResultSaver saver = asHtml
            ? new HtmlResultSaver(xsl ?? throw new InvalidOperationException("Спочатку відкрийте XSL-файл"))
            : new XmlResultSaver();

        string saved = await saver.SaveAsync(Storage(place), name, found);
        string where = place == Place.Drive ? " на Google Drive" : "";
        if (asHtml)
        {
            EventLog.Instance.Write(EventLevel.Transform, $"Збережено у файл {Path.GetFileName(saved)}{where}");
        }
        else
        {
            EventLog.Instance.Write(EventLevel.Save,
                $"Збережено відфільтрований фрагмент у файл {Path.GetFileName(saved)}{where}");
        }
        return saved;
    }

    // Копіює відкриті XML і XSL на Google Drive, щоб потім відкривати їх звідти
    public async Task<List<string>> UploadSourcesToDriveAsync()
    {
        var uploaded = new List<string>();
        if (xml != null && XmlName != null)
        {
            await RequireDrive().SaveAsync(XmlName, Encoding.UTF8.GetBytes(xml), "application/xml");
            uploaded.Add(XmlName);
        }
        if (xsl != null && XslName != null)
        {
            await RequireDrive().SaveAsync(XslName, Encoding.UTF8.GetBytes(xsl), "application/xml");
            uploaded.Add(XslName);
        }
        if (uploaded.Count == 0)
        {
            throw new InvalidOperationException("Спочатку відкрийте XML або XSL-файл");
        }
        EventLog.Instance.Write(EventLevel.Save, $"На Google Drive збережено: {string.Join(", ", uploaded)}");
        return uploaded;
    }

    public Task<List<string>> GetDriveFileNamesAsync(string extension)
        => RequireDrive() is IListableStorage listable
            ? listable.ListAsync(extension)
            : throw new InvalidOperationException("Це сховище не показує список файлів");

    private async Task<string> LoadTextAsync(Place place, string name)
    {
        byte[] bytes = await Storage(place).LoadAsync(name);
        using var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8);   // сам прибирає BOM
        return await reader.ReadToEndAsync();
    }

    private IFileStorage Storage(Place place) => place == Place.Drive ? RequireDrive() : fileStorage;

    private IFileStorage RequireDrive()
        => driveStorage ?? throw new InvalidOperationException("Google Drive не підключено");
}

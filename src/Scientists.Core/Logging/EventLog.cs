namespace Scientists.Core.Logging;

// Рівні важливості подій з умови 2*
public enum EventLevel
{
    Filter,
    Transform,
    Save,
}

// Одинак: один журнал на всю програму, доступ через EventLog.Instance.
// Конструктор приватний, тому другий об'єкт створити не можна
public sealed class EventLog
{
    private static readonly Lazy<EventLog> instance = new(() => new EventLog());
    private readonly object fileLock = new();

    private EventLog()
    {
    }

    public static EventLog Instance => instance.Value;

    // Файл журналу задає UI при запуску; поки null - події нікуди не пишуться
    public string? FilePath { get; set; }

    public void Write(EventLevel level, string message)
    {
        string? path = FilePath;
        if (path == null) return;
        string line = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {LevelName(level)}. {message}{Environment.NewLine}";
        lock (fileLock)   // кнопки можуть спрацювати одночасно, а писати у файл треба по черзі
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.AppendAllText(path, line);
        }
    }

    public static string LevelName(EventLevel level) => level switch
    {
        EventLevel.Filter => "Фільтрація",
        EventLevel.Transform => "Трансформація",
        _ => "Збереження",
    };
}

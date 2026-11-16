namespace Scientists.Core.Search;

// Що обрав користувач: атрибут -> значення. Атрибута немає в словнику - підходить будь-яке значення
public class SearchCriteria
{
    private readonly Dictionary<string, string> values = new();

    public IReadOnlyDictionary<string, string> Values => values;

    public SearchCriteria With(string attribute, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            values.Remove(attribute);
        }
        else
        {
            values[attribute] = value;
        }
        return this;
    }

    // Запис підходить, якщо збігаються всі обрані атрибути
    public bool Matches(IReadOnlyDictionary<string, string> attributes)
        => values.All(c => attributes.TryGetValue(c.Key, out string? v) && v == c.Value);

    // Для журналу: «Faculty» – ФКНК, «Position» – доцент
    public override string ToString()
        => values.Count == 0 ? "без умов" : string.Join(", ", values.Select(c => $"«{c.Key}» – {c.Value}"));
}

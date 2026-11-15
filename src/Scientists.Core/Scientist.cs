using System.Xml.Linq;

namespace Scientists.Core;

// Один запис з XML: <Scientist FullName="..." Faculty="..." ... />.
// Degree (науковий ступінь) є не в усіх записах, тому кількість атрибутів у вузлів різна
public record Scientist(string FullName, string Faculty, string Department, string Position, string Salary, string Since,
    string? Degree = null)
{
    public const string RootName = "Scientists";
    public const string ElementName = "Scientist";

    // Атрибути, за якими можна шукати, і їхні підписи в інтерфейсі
    public static readonly IReadOnlyDictionary<string, string> SearchAttributes = new Dictionary<string, string>
    {
        ["Faculty"] = "Факультет",
        ["Department"] = "Кафедра",
        ["Position"] = "Посада",
    };

    // Усі три стратегії читають атрибути в словник і створюють запис через цей метод
    public static Scientist FromAttributes(IReadOnlyDictionary<string, string> a) => new(
        a.GetValueOrDefault("FullName", ""),
        a.GetValueOrDefault("Faculty", ""),
        a.GetValueOrDefault("Department", ""),
        a.GetValueOrDefault("Position", ""),
        a.GetValueOrDefault("Salary", ""),
        a.GetValueOrDefault("Since", ""),
        a.GetValueOrDefault("Degree"));

    public XElement ToXml() => new(ElementName,
        new XAttribute("FullName", FullName),
        new XAttribute("Faculty", Faculty),
        new XAttribute("Department", Department),
        new XAttribute("Position", Position),
        new XAttribute("Salary", Salary),
        new XAttribute("Since", Since),
        Degree == null ? null : new XAttribute("Degree", Degree));   // немає ступеня - немає атрибута

    // Відфільтровані записи як XML-документ того ж формату, що й вхідний (тому до нього підходить той самий XSL)
    public static XDocument ToDocument(IEnumerable<Scientist> scientists)
        => new(new XElement(RootName, scientists.Select(s => s.ToXml())));
}

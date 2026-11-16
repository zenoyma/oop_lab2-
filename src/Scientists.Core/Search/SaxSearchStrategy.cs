using System.Xml;

namespace Scientists.Core.Search;

// SAX: читаємо файл послідовно, вузол за вузлом, не тримаючи весь документ у пам'яті.
// У .NET немає класичного SAX з подіями, його аналог - XmlReader (так само йде вперед по одному вузлу)
public class SaxSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX API";

    public List<Scientist> Search(string xml, SearchCriteria criteria)
    {
        var result = new List<Scientist>();
        using var reader = XmlReader.Create(new StringReader(xml));
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != Scientist.ElementName)
            {
                continue;
            }
            var attributes = new Dictionary<string, string>();
            while (reader.MoveToNextAttribute())
            {
                attributes[reader.Name] = reader.Value;
            }
            if (criteria.Matches(attributes))
            {
                result.Add(Scientist.FromAttributes(attributes));
            }
        }
        return result;
    }
}

using System.Xml;

namespace Scientists.Core.Search;

// DOM: спочатку весь документ завантажується в пам'ять деревом вузлів, потім ходимо по ньому
public class DomSearchStrategy : IXmlSearchStrategy
{
    public string Name => "DOM API";

    public List<Scientist> Search(string xml, SearchCriteria criteria)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);

        var result = new List<Scientist>();
        foreach (XmlElement element in document.GetElementsByTagName(Scientist.ElementName))
        {
            var attributes = new Dictionary<string, string>();
            foreach (XmlAttribute attribute in element.Attributes)
            {
                attributes[attribute.Name] = attribute.Value;
            }
            if (criteria.Matches(attributes))
            {
                result.Add(Scientist.FromAttributes(attributes));
            }
        }
        return result;
    }
}

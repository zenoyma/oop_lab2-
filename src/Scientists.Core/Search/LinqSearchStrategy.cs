using System.Xml.Linq;

namespace Scientists.Core.Search;

// LINQ to XML: документ теж у пам'яті (XDocument), а пошук записується одним запитом
public class LinqSearchStrategy : IXmlSearchStrategy
{
    public string Name => "LINQ to XML";

    public List<Scientist> Search(string xml, SearchCriteria criteria)
        => XDocument.Parse(xml)
            .Descendants(Scientist.ElementName)
            .Select(e => e.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value))
            .Where(criteria.Matches)
            .Select(Scientist.FromAttributes)
            .ToList();
}

namespace Scientists.Core.Search;

// Стратегія: один спосіб розбору XML. Контролер не знає, яка саме - просто викликає Search
public interface IXmlSearchStrategy
{
    string Name { get; }

    List<Scientist> Search(string xml, SearchCriteria criteria);
}

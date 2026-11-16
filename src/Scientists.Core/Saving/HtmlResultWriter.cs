using System.Xml;
using System.Xml.Xsl;

namespace Scientists.Core.Saving;

// Відфільтровані записи -> XML -> HTML через XSL-файл користувача
public class HtmlResultWriter : IResultWriter
{
    private readonly string xsl;

    public HtmlResultWriter(string xsl)
    {
        this.xsl = xsl;
    }

    public string Extension => ".html";

    public string MimeType => "text/html";

    public void Write(Stream output, IReadOnlyList<Scientist> results)
    {
        var transform = new XslCompiledTransform();
        using (var xslReader = XmlReader.Create(new StringReader(xsl)))
        {
            transform.Load(xslReader);   // кидає XsltException, якщо XSL з помилкою
        }
        using var input = Scientist.ToDocument(results).CreateReader();
        transform.Transform(input, null, output);
    }
}

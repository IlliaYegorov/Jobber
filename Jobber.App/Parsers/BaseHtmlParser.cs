using HtmlAgilityPack;

namespace Jobber.App.Parsers;

public interface IHtmlParser<T> where T : class
{
    /// <summary>
    /// Loads HTML content into the parser.
    /// </summary>
    /// <param name="htmlContent">The raw HTML content as a string.</param>
    void LoadHtml(string htmlContent);

    /// <summary>
    /// Parses specific information from the HTML.
    /// </summary>
    /// <typeparam name="T">The type of the parsed result.</typeparam>
    /// <returns>The parsed result of type T.</returns>
    IEnumerable<T> Parse();
}

public abstract class BaseHtmlParser<T> : IHtmlParser<T> where T : class
{
    protected HtmlDocument HtmlDocument { get; private set; } = new HtmlDocument();

    /// <summary>
    /// Loads HTML content into the parser.
    /// </summary>
    /// <param name="htmlContent">The raw HTML content as a string.</param>
    public void LoadHtml(string htmlContent)
    {
        HtmlDocument = new HtmlDocument();
        HtmlDocument.LoadHtml(htmlContent);
    }

    /// <summary>
    /// Retrieves all elements by the specified tag name.
    /// </summary>
    /// <param name="tagName">The name of the HTML tag.</param>
    /// <returns>A collection of HtmlNode elements.</returns>
    protected IEnumerable<HtmlNode> GetElementsByTag(string tagName)
    {
        return HtmlDocument.DocumentNode.Descendants(tagName);
    }

    /// <summary>
    /// Retrieves elements by a specific attribute and its value.
    /// </summary>
    /// <param name="attributeName">The attribute name (e.g., "class").</param>
    /// <param name="attributeValue">The attribute value to match.</param>
    /// <returns>A collection of matching HtmlNode elements.</returns>
    protected IEnumerable<HtmlNode> GetElementsByAttribute(string attributeName, string attributeValue, string elementTag)
    {
        return HtmlDocument.DocumentNode
            .Descendants(elementTag)
            .Where(node => node.GetAttributeValue(attributeName, "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains(attributeValue));
    }

    /// <summary>
    /// Retrieves the inner text of a node by XPath.
    /// </summary>
    /// <param name="xPath">The XPath expression.</param>
    /// <returns>The inner text of the matched node, or null if not found.</returns>
    protected string? GetInnerTextByXPath(string xPath)
    {
        var node = HtmlDocument.DocumentNode.SelectSingleNode(xPath);
        return node?.InnerText.Trim();
    }

    /// <summary>
    /// Retrieves an attribute value from a node by XPath.
    /// </summary>
    /// <param name="xPath">The XPath expression.</param>
    /// <param name="attributeName">The attribute name (e.g., "href").</param>
    /// <returns>The attribute value, or null if not found.</returns>
    protected string? GetAttributeByXPath(string xPath, string attributeName)
    {
        var node = HtmlDocument.DocumentNode.SelectSingleNode(xPath);
        return node?.GetAttributeValue(attributeName, null);
    }

    /// <summary>
    /// Retrieves all hyperlinks (anchor tags) from the HTML that match the specified attribute name and value, 
    /// including both the href attribute value and the inner text of each link.
    /// </summary>
    /// <param name="attributeName">The name of the attribute to filter the anchor tags by.</param>
    /// <param name="attributeValue">The value of the attribute to match.</param>
    /// <returns>A collection of tuples containing the URL and text content of matching anchor tags.</returns>
    protected IEnumerable<(string Url, string Text)> GetHyperlinksWithTextByAttribute(string attributeName, string attributeValue)
    {
        return HtmlDocument.DocumentNode
            .Descendants("a")
            .Where(node =>
                node.GetAttributeValue(attributeName, "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains(attributeValue))
            .Select(node => (
                Url: node.GetAttributeValue("href", string.Empty),
                Text: HtmlEntity.DeEntitize(node.InnerText.Trim())
            ))
            .Where(link => !string.IsNullOrEmpty(link.Url));
    }

    /// <summary>
    /// Retrieves all text content from the document.
    /// </summary>
    /// <returns>A concatenated string of all text content.</returns>
    protected string GetAllTextContent()
    {
        return string.Join(" ", HtmlDocument.DocumentNode
            .Descendants()
            .Where(node => !string.IsNullOrWhiteSpace(node.InnerText))
            .Select(node => node.InnerText.Trim()));
    }

    /// <summary>
    /// Removes HTML entities and returns plain text.
    /// </summary>
    /// <param name="html">The input HTML string.</param>
    /// <returns>The plain text with HTML entities decoded.</returns>
    protected static string RemoveHtmlTags(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var plainText = doc.DocumentNode.InnerText;

        return HtmlEntity.DeEntitize(plainText);
    }

    /// <summary>
    /// Abstract method to parse specific information from the HTML.
    /// Subclasses must implement this based on specific parsing needs.
    /// </summary>
    /// <typeparam name="T">The type of the parsed result.</typeparam>
    /// <returns>The parsed result of type T.</returns>
    public abstract IEnumerable<T> Parse();
}

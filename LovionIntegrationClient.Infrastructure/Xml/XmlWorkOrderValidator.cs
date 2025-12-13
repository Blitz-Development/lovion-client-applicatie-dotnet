using System.Xml;
using System.Xml.Schema;

namespace LovionIntegrationClient.Infrastructure.Xml;

public class XmlWorkOrderValidator
{
    private readonly XmlSchemaSet _schemas;

    public XmlWorkOrderValidator()
    {
        _schemas = new XmlSchemaSet();

        // Pad naar workorders.xsd in de output-map
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "XmlSchemas", "workorders.xsd");

        using var stream = File.OpenRead(schemaPath);
        using var reader = XmlReader.Create(stream);

        // Namespace moet gelijk zijn aan targetNamespace in workorders.xsd
        _schemas.Add("http://www.loviondummy.nl/workorders", reader);
    }

    public ValidationResult Validate(string xml)
    {
        var result = new ValidationResult();

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = _schemas
        };

        settings.ValidationEventHandler += (_, args) =>
        {
            // Elke schemafout komt hier binnen
            result.Errors.Add(args.Message);
        };

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, settings);

        try
        {
            // Hele document lezen; validatie-event fire’t tijdens het lezen
            while (xmlReader.Read()) { }
        }
        catch (XmlException ex)
        {
            // Fout in de XML zelf (niet eens geldig XML)
            result.Errors.Add($"XML parse error: {ex.Message}");
        }

        return result;
    }
}
using System.Xml;
using System.Xml.Schema;

namespace LovionIntegrationClient.Infrastructure.Xml;

public class XmlWorkOrderValidator
{
    private readonly XmlSchemaSet _schemas;

    public XmlWorkOrderValidator()
    {
        _schemas = new XmlSchemaSet
        {
            XmlResolver = new XmlUrlResolver()
        };

        var schemaPath = Path.Combine(AppContext.BaseDirectory, "XmlSchemas", "workorders.xsd");

        // Belangrijk: Add met pad zodat includes/imports relative aan dit bestand werken
        _schemas.Add(null, schemaPath);

        _schemas.Compile();
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
            if (args.Severity == XmlSeverityType.Warning)
                result.Warnings.Add(args.Message);
            else
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
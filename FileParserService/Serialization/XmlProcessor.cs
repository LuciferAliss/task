using System.Xml.Serialization;

namespace FileParserService.Serialization;

public static class XmlProcessor
{
    public static T? Deserialize<T>(Stream stream) where T : class
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return xmlSerializer.Deserialize(stream) as T;
    }

    public static T? Deserialize<T>(TextReader reader) where T : class
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return xmlSerializer.Deserialize(reader) as T;
    }
}
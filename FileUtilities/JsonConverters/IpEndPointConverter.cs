using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluffyVoid.FileUtilities.JsonConverters;

/// <summary>
///     Custom converter for IpEndPoint for use with the NewtonSoft.Json library
/// </summary>
public class IpEndPointConverter : JsonConverter
{
    /// <summary>
    ///     Whether the converter is capable of converting the incoming type or not
    /// </summary>
    /// <param name="objectType">The type of the object that needs to be converted</param>
    /// <returns>True if the converter can convert the object, otherwise false</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IPEndPoint);
    }
    /// <summary>
    ///     Parses the IPEndPoint from the json reader
    /// </summary>
    /// <param name="reader">Reference to the json reader</param>
    /// <param name="objectType">The type of the object to read</param>
    /// <param name="existingValue">The existing value</param>
    /// <param name="serializer">Reference to the json serializer</param>
    /// <returns>The parsed IpEndPoint</returns>
    public override object? ReadJson(JsonReader reader, Type objectType,
                                     object? existingValue,
                                     JsonSerializer serializer)
    {
        string jsonString = JToken.Load(reader).ToString();
        if (string.IsNullOrEmpty(jsonString))
        {
            throw new
                NullReferenceException("JToken has read in a null or empty string, unable to use data to parse into an IP EndPoint.");
        }

        string[] components = jsonString.Split(':');
        if (components.Length < 2 || !int.TryParse(components[1], out int port))
        {
            return null;
        }

        return new IPEndPoint(IPAddress.Parse(components[0]), port);
    }
    /// <summary>
    ///     Writes the IPEndPoint to the json writer
    /// </summary>
    /// <param name="writer">Reference to the json writer</param>
    /// <param name="value">The value to write out</param>
    /// <param name="serializer">Reference to the json serializer</param>
    public override void WriteJson(JsonWriter writer, object? value,
                                   JsonSerializer serializer)
    {
        if (value is not IPEndPoint ipEndPoint)
        {
            throw new
                ArgumentException("Data is not an IPEndPoint. Unable to write to JSON.");
        }

        if (ipEndPoint.Address == null && ipEndPoint.Port == 0)
        {
            writer.WriteNull();
            return;
        }

        JToken.FromObject($"{ipEndPoint.Address}:{ipEndPoint.Port}")
              .WriteTo(writer);
    }
}
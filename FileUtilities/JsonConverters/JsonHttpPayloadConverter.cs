using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluffyVoid.FileUtilities.JsonConverters;

/// <summary>
///     Custom converter for JSON api request payloads for use with the NewtonSoft.Json library
/// </summary>
public class JsonHttpPayloadConverter : JsonConverter
{
    /// <summary>
    ///     Whether the converter is capable of converting the incoming type or not
    /// </summary>
    /// <param name="objectType">The type of the object that needs to be converted</param>
    /// <returns>True if the converter can convert the object, otherwise false</returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
    }
    /// <summary>
    ///     Parses the JSON api request from the json reader into a Dictionary
    /// </summary>
    /// <param name="reader">Reference to the json reader</param>
    /// <param name="objectType">The type of the object to read</param>
    /// <param name="existingValue">The existing value</param>
    /// <param name="serializer">Reference to the json serializer</param>
    /// <returns>The parsed IPAddress</returns>
    /// <exception cref="NotImplementedException">
    ///     Exception indicating that there was no way to parse the incoming data into
    ///     the specified object type
    /// </exception>
    public override object? ReadJson(JsonReader reader, Type objectType,
                                     object? existingValue,
                                     JsonSerializer serializer)
    {
        return ReadValue(reader);
    }
    /// <summary>
    ///     Writes the Dictionary to the json writer
    /// </summary>
    /// <param name="writer">Reference to the json writer</param>
    /// <param name="value">The value to write out</param>
    /// <param name="serializer">Reference to the json serializer</param>
    /// <exception cref="NotImplementedException">
    ///     Exception indicating that there was no way to parse the incoming data into
    ///     the specified object type
    /// </exception>
    public override void WriteJson(JsonWriter writer, object? value,
                                   JsonSerializer serializer)
    {
        WriteValue(writer, value);
    }

    /// <summary>
    ///     Helper function used to read out an array type to place into the value of a dictionary entry
    /// </summary>
    /// <param name="reader">Reference to the json reader</param>
    /// <returns>The array object to place into the dictionary</returns>
    private object? ReadArray(JsonReader reader)
    {
        IList<object> list = new List<object>();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                    break;
                default:
                    object? value = ReadValue(reader);
                    if (value == null)
                    {
                        continue;
                    }

                    list.Add(value);
                    break;
                case JsonToken.EndArray:
                    return list;
            }
        }

        return null;
    }
    /// <summary>
    ///     Helper function used to read out a dictionary value to place into the value of a dictionary entry
    /// </summary>
    /// <param name="reader">Reference to the json reader</param>
    /// <returns>The dictionary object to place into the dictionary</returns>
    private object? ReadObject(JsonReader reader)
    {
        Dictionary<string, object> dictionaryObject =
            new Dictionary<string, object>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    if (reader.Value == null)
                    {
                        continue;
                    }

                    string propertyName = reader.Value.ToString();
                    if (!reader.Read())
                    {
                        return null;
                    }

                    object? value = ReadValue(reader);
                    dictionaryObject[propertyName] = value!;
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    return dictionaryObject;
            }
        }

        return null;
    }
    /// <summary>
    ///     Helper function used to read out the varying data types to place into the value of a dictionary entry
    /// </summary>
    /// <param name="reader">Reference to the json reader</param>
    /// <returns>The populated dictionary to return to the user</returns>
    private object? ReadValue(JsonReader reader)
    {
        while (reader.TokenType == JsonToken.Comment)
        {
            if (!reader.Read())
            {
                return null;
            }
        }

        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return ReadObject(reader);
            case JsonToken.StartArray:
                return ReadArray(reader);
            case JsonToken.Date:
                return reader.Value is DateTime date
                    ? date.ToString("s")
                    : string.Empty;
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Undefined:
            case JsonToken.Null:
            case JsonToken.Bytes:
                return reader.Value;
            default:
                return null;
        }
    }
    /// <summary>
    ///     Helper function used to write an array into a JSON api payload
    /// </summary>
    /// <param name="writer">Reference to the json writer</param>
    /// <param name="value">The value to write out</param>
    private void WriteArray(JsonWriter writer, object value)
    {
        writer.WriteStartArray();
        if (value is not IEnumerable<object> arrayObject)
        {
            writer.WriteEndArray();
            return;
        }

        foreach (object currentObject in arrayObject)
        {
            WriteValue(writer, currentObject);
        }

        writer.WriteEndArray();
    }
    /// <summary>
    ///     Helper function used to write a dictionary into a JSON api payload
    /// </summary>
    /// <param name="writer">Reference to the json writer</param>
    /// <param name="value">The value to write out</param>
    private void WriteObject(JsonWriter writer, object value)
    {
        writer.WriteStartObject();
        if (value is not IDictionary<string, object> dictionaryObject)
        {
            writer.WriteEndObject();
            return;
        }

        foreach (KeyValuePair<string, object> currentObject in dictionaryObject)
        {
            writer.WritePropertyName(currentObject.Key);
            WriteValue(writer, currentObject.Value);
        }

        writer.WriteEndObject();
    }
    /// <summary>
    ///     Helper function used to write the varying data types into a JSON api payload
    /// </summary>
    /// <param name="writer">Reference to the json writer</param>
    /// <param name="value">The value to write out</param>
    private void WriteValue(JsonWriter writer, object? value)
    {
        if (value == null)
        {
            return;
        }

        JToken t = JToken.FromObject(value);
        switch (t.Type)
        {
            case JTokenType.Object:
                WriteObject(writer, value);
                break;
            case JTokenType.Array:
                WriteArray(writer, value);
                break;
            default:
                writer.WriteValue(value);
                break;
        }
    }
}
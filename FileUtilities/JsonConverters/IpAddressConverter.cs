using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluffyVoid.FileUtilities.JsonConverters;

/// <summary>
///     Custom converter for IpAddresses for use with the NewtonSoft.Json library
/// </summary>
public class IpAddressConverter : JsonConverter
{
    private const string InvalidArgumentMessage =
        "Input object type is not a valid IP Address or list of IP Addresses. Unable to read object from JSON.";
    /// <summary>
    ///     Whether the converter is capable of converting the incoming type or not
    /// </summary>
    /// <param name="objectType">The type of the object that needs to be converted</param>
    /// <returns>True if the converter can convert the object, otherwise false</returns>
    public override bool CanConvert(Type objectType)
    {
        if (objectType == typeof(IPAddress))
        {
            return true;
        }

        if (objectType == typeof(List<IPAddress>))
        {
            return true;
        }

        return false;
    }
    /// <summary>
    ///     Parses the IPAddress from the json reader
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
    public override object ReadJson(JsonReader reader, Type objectType,
                                    object? existingValue,
                                    JsonSerializer serializer)
    {
        // convert an ipaddress represented as a string into an IPAddress object
        // and return it to the caller
        if (objectType == typeof(IPAddress))
        {
            if (!IPAddress.TryParse(JToken.Load(reader).ToString(),
                                    out IPAddress result))
            {
                throw new
                    InvalidOperationException("Unable to parse IP Address from the current JToken.");
            }

            return result;
        }

        // convert a json array of ip addresses represented as strings into a
        // List<IPAddress> object and return it to the caller
        if (objectType == typeof(List<IPAddress>))
        {
            return JToken.Load(reader)
                         .Select(token =>
                                     IPAddress.TryParse(token.ToString(),
                                              out IPAddress address)
                                         ? address
                                         : IPAddress.None).ToList();
        }

        throw new ArgumentException(InvalidArgumentMessage);
    }
    /// <summary>
    ///     Writes the IPAddress to the json writer
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
        if (value == null)
        {
            throw new NullReferenceException();
        }

        // convert an IPAddress object to a string representation of itself and
        // write it to the serializer
        if (value.GetType() == typeof(IPAddress))
        {
            JToken.FromObject(value.ToString()).WriteTo(writer);
            return;
        }

        // convert a List<IPAddress> object to an array of strings of ip
        // addresses and write it to the serializer
        if (value.GetType() == typeof(List<IPAddress>))
        {
            JToken.FromObject((from n in (List<IPAddress>)value
                               select n.ToString()).ToList()).WriteTo(writer);
        }

        throw new ArgumentException(InvalidArgumentMessage);
    }
}
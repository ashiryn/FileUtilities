using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Formatting = Newtonsoft.Json.Formatting;

namespace FluffyVoid.FileUtilities.DataLoader;

/// <summary>
///     Loads data from a dynamic data path allowing users to define primary and fallback data locations
/// </summary>
public class DataLoader
{
    /// <summary>
    ///     Lookup table of additional fallback paths to use if the primary data path fails
    /// </summary>
    private static readonly HashSet<IDataLoaderPath>
        FallbackDataLoaderPaths = new HashSet<IDataLoaderPath>();
    /// <summary>
    ///     The primary data path to use in saving/loading files
    /// </summary>
    private static IDataLoaderPath s_primaryDataLoaderPath =
        new DataLoaderPath();

    /// <summary>
    ///     The primary data loader path to use when performing IO operations
    /// </summary>
    public static IDataLoaderPath? PrimaryDataLoaderPath
    {
        set => s_primaryDataLoaderPath = value ?? new DataLoaderPath();
    }

    /// <summary>
    ///     Adds a fallback path to use when performing IO operations
    /// </summary>
    /// <param name="path">The data loader path to add</param>
    public static void AddFallbackPath(IDataLoaderPath path)
    {
        FallbackDataLoaderPaths.Add(path);
    }
    /// <summary>
    ///     Initializes the Data Loader with paths to use during IO operations
    /// </summary>
    /// <param name="primaryPath">The primary data loader path to use for all IO operations</param>
    /// <param name="fallbackPaths">Additional fallback data loader paths to use for all IO operations</param>
    public static void Initialize(IDataLoaderPath primaryPath,
                                  params IDataLoaderPath[] fallbackPaths)
    {
        s_primaryDataLoaderPath = primaryPath;
        foreach (IDataLoaderPath dataLoaderPath in fallbackPaths)
        {
            FallbackDataLoaderPaths.Add(dataLoaderPath);
        }
    }
    /// <summary>
    ///     Utility function to try and load a JSON file from disk, prepending the file path with DataPath
    /// </summary>
    /// <param name="filePath">Path to the JSON file within our Data folder</param>
    /// <param name="data">The loaded JSON data built from the JSON file</param>
    /// <param name="location">The data location type to load the JSON file from</param>
    /// <param name="createIfMissing">
    ///     Whether the data loader should create a default instance of the data and file if it does not exist
    /// </param>
    /// <typeparam name="T">The type of data to load from the JSON file</typeparam>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    public static bool LoadJsonData<T>(string filePath, out T? data,
                                       DataLocation location =
                                           DataLocation.ApplicationData,
                                       bool createIfMissing = false)
    {
        data = default;
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        if (LoadJsonData(fullFilePath, ref data))
        {
            return true;
        }

        foreach (IDataLoaderPath fallbackPath in FallbackDataLoaderPaths)
        {
            fullFilePath = $"{fallbackPath.GetDataPath(location)}/{filePath}";
            if (LoadJsonData(fullFilePath, ref data))
            {
                return true;
            }
        }

        if (createIfMissing)
        {
            data = Activator.CreateInstance<T>();
            SaveJsonData($"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}",
                         data);
        }

        return false;
    }
    /// <summary>
    ///     Utility function to try and load a Text file from disk, prepending the file path with DataPath
    /// </summary>
    /// <param name="filePath">Path to the Text file within our Data folder</param>
    /// <param name="data">The text loaded from the file</param>
    /// <param name="location">The data location type to load the Text file from</param>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    public static bool LoadTextFile(string filePath, out string data,
                                    DataLocation location =
                                        DataLocation.ApplicationData)
    {
        data = string.Empty;
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        if (ReadAllText(fullFilePath, out data))
        {
            return true;
        }

        foreach (IDataLoaderPath fallbackPath in FallbackDataLoaderPaths)
        {
            fullFilePath = $"{fallbackPath.GetDataPath(location)}/{filePath}";
            if (ReadAllText(fullFilePath, out data))
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    ///     Utility function to try and load an XML file from disk, prepending the file path with DataPath
    /// </summary>
    /// <param name="filePath">Path to the XML file within our Data folder</param>
    /// <param name="data">The loaded Xml node list built from the XML file</param>
    /// <param name="location">The data location type to load the XML file from</param>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    public static bool LoadXml(string filePath,
                               out XmlNodeList? data,
                               DataLocation location =
                                   DataLocation.ApplicationData)
    {
        data = null;
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        if (ReadAllXml(fullFilePath, out data))
        {
            return true;
        }

        foreach (IDataLoaderPath fallbackPath in FallbackDataLoaderPaths)
        {
            fullFilePath = $"{fallbackPath.GetDataPath(location)}/{filePath}";
            if (ReadAllXml(fullFilePath, out data))
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    ///     Utility function to try and load a Yaml file from disk, prepending the file path with DataPath
    /// </summary>
    /// <param name="filePath">Path to the Yaml file within our Data folder</param>
    /// <param name="data">The loaded yaml data built from the Yaml file</param>
    /// <param name="location">The data location type to load the Yaml file from</param>
    /// <param name="namingConvention">The naming convention the Yaml file is using</param>
    /// <param name="createIfMissing">
    ///     Whether the data loader should create a default instance of the data and file if it does not exist
    /// </param>
    /// <typeparam name="T">The type of data to load from the yaml file</typeparam>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    public static bool LoadYaml<T>(string filePath, out T? data,
                                   DataLocation location =
                                       DataLocation.ApplicationData,
                                   INamingConvention? namingConvention =
                                       null, bool createIfMissing = false)
    {
        data = default;
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        if (namingConvention == null)
        {
            namingConvention = UnderscoredNamingConvention.Instance;
        }

        if (LoadYamlData(fullFilePath, namingConvention, ref data))
        {
            return true;
        }

        foreach (IDataLoaderPath fallbackPath in FallbackDataLoaderPaths)
        {
            fullFilePath = $"{fallbackPath.GetDataPath(location)}/{filePath}";
            if (LoadYamlData(fullFilePath, namingConvention, ref data))
            {
                return true;
            }
        }

        if (createIfMissing)
        {
            data = Activator.CreateInstance<T>();
            SaveYamlData($"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}",
                         data);
        }

        return false;
    }
    /// <summary>
    ///     Removes a fallback path
    /// </summary>
    /// <param name="path">The data loader path to remove</param>
    public static void RemoveFallbackPath(IDataLoaderPath path)
    {
        FallbackDataLoaderPaths.Remove(path);
    }
    /// <summary>
    ///     Utility function to try and save a JSON file to disk, prepending the file path with DataPath
    /// </summary>
    /// <typeparam name="T">Type of the object to try and save to the JSON file</typeparam>
    /// <param name="filePath">Path to the JSON file within our Data folder</param>
    /// <param name="data">The object that you wish to save via JSON</param>
    /// <param name="location">The data location type to save the JSON file to</param>
    /// <returns>True if the save operation was successful, otherwise false</returns>
    public static bool SaveJsonData<T>(string filePath, T data,
                                       DataLocation location =
                                           DataLocation.ApplicationData)
    {
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        try
        {
            if (!Path.HasExtension(fullFilePath))
            {
                fullFilePath += ".json";
            }

            if (!Directory.Exists(Path.GetDirectoryName(fullFilePath)))
            {
                new FileInfo(fullFilePath).Directory?.Create();
            }

            File.WriteAllText(fullFilePath,
                              JsonConvert.SerializeObject(data,
                                       Formatting.Indented));
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Utility function to try and save a Text file to disk, prepending the file path with DataPath
    /// </summary>
    /// <param name="filePath">Path to the Text file within our Data folder</param>
    /// <param name="data">The text to save out to file</param>
    /// <param name="location">The data location type to save the Text file to</param>
    /// <param name="append">Determines if the text should be appended to the existing file or to overwrite the file</param>
    public static bool SaveTextFile(string filePath, string data,
                                    DataLocation location =
                                        DataLocation.ApplicationData,
                                    bool append = false)
    {
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        try
        {
            if (!Path.HasExtension(fullFilePath))
            {
                fullFilePath += ".txt";
            }

            if (!Directory.Exists(Path.GetDirectoryName(fullFilePath)))
            {
                new FileInfo(fullFilePath).Directory?.Create();
            }

            if (append && ReadAllText(fullFilePath, out string loaded))
            {
                data = $"{loaded}\n{data}";
            }

            File.WriteAllText(fullFilePath, data);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Utility function to try and save a Yaml file to disk, prepending the file path with DataPath
    /// </summary>
    /// <typeparam name="T">Type of the object to try and save to the Yaml file</typeparam>
    /// <param name="filePath">Path to the Yaml file within our Data folder</param>
    /// <param name="data">The object that you wish to save via Yaml</param>
    /// <param name="location">The data location type to save the Yaml file to</param>
    /// <param name="namingConvention">The naming convention the Yaml file should use</param>
    /// <returns>True if the save operation was successful, otherwise false</returns>
    public static bool SaveYamlData<T>(string filePath, T data,
                                       DataLocation location =
                                           DataLocation.ApplicationData,
                                       INamingConvention? namingConvention =
                                           null)
    {
        string fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        try
        {
            if (!Path.HasExtension(fullFilePath))
            {
                fullFilePath += ".yaml";
            }

            if (!Directory.Exists(Path.GetDirectoryName(fullFilePath)))
            {
                new FileInfo(fullFilePath).Directory?.Create();
            }

            ISerializer serializer = new SerializerBuilder()
                                     .WithNamingConvention(namingConvention ??
                                              UnderscoredNamingConvention
                                                  .Instance).Build();

            File.WriteAllText(fullFilePath, serializer.Serialize(data));
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Attempts to find the full file path for the passed in file path by iterating over all the loaded and available data
    ///     paths
    /// </summary>
    /// <param name="filePath">Path to the file within our Data folder</param>
    /// <param name="location">The data location type to try to find a file path for</param>
    /// <param name="fullFilePath">The full data path if the path was found in any available data loader paths</param>
    /// <returns>True if the file path exists in any data loader path, otherwise false</returns>
    public static bool TryFindFullPath(string filePath, DataLocation location,
                                       out string fullFilePath)
    {
        fullFilePath =
            $"{s_primaryDataLoaderPath.GetDataPath(location)}/{filePath}";

        if (File.Exists(fullFilePath))
        {
            return true;
        }

        foreach (IDataLoaderPath fallbackPath in FallbackDataLoaderPaths)
        {
            fullFilePath = $"{fallbackPath.GetDataPath(location)}/{filePath}";
            if (File.Exists(fullFilePath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Helper function used to load JSON data from a file path
    /// </summary>
    /// <param name="filePath">Path to the JSON file within our Data folder</param>
    /// <param name="data">The loaded JSON data built from the JSON file</param>
    /// <typeparam name="T">The type of data to load from the JSON file</typeparam>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    private static bool LoadJsonData<T>(string filePath, ref T? data)
    {
        if (!Path.HasExtension(filePath))
        {
            filePath += ".json";
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            string jsonDataString = File.ReadAllText(filePath);
            data = JsonConvert.DeserializeObject<T>(jsonDataString);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Helper function used to load Yaml data from a file path
    /// </summary>
    /// <param name="filePath">The full file path to the JSON file</param>
    /// <param name="data">The loaded yaml data built from the yaml file</param>
    /// <param name="namingConvention">The naming convention the yaml file is using</param>
    /// <typeparam name="T">The type of data to load from the yaml file</typeparam>
    /// <returns>True if the load operation was successful, otherwise false</returns>
    private static bool LoadYamlData<T>(string filePath,
                                        INamingConvention namingConvention,
                                        ref T? data)
    {
        if (!Path.HasExtension(filePath))
        {
            filePath += ".yaml";
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            string yamlDataString = File.ReadAllText(filePath);
            IDeserializer deserializer = new DeserializerBuilder()
                                         .WithNamingConvention(namingConvention)
                                         .Build();

            data = deserializer.Deserialize<T>(yamlDataString);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Helper function that performs the logic of reading all the text from a text file, without logging
    /// </summary>
    /// <param name="filePath">Full path to the text file within our Data folder</param>
    /// <param name="data">The text loaded from the file</param>
    /// <returns>True if the read operation was successful, otherwise false</returns>
    private static bool ReadAllText(string filePath, out string data)
    {
        data = string.Empty;
        if (!Path.HasExtension(filePath))
        {
            filePath += ".txt";
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            data = File.ReadAllText(filePath);
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Helper function that performs the logic of reading the XML file
    /// </summary>
    /// <param name="filePath">Full path to the XML file within our Data folder</param>
    /// <param name="data">The loaded Xml node list built from the XML file</param>
    /// <returns>True if the read operation was successful, otherwise false</returns>
    private static bool ReadAllXml(string filePath,
                                   out XmlNodeList? data)
    {
        data = null;
        XmlDocument document = new XmlDocument();
        if (!Path.HasExtension(filePath))
        {
            filePath += ".xml";
        }

        if (!ReadAllText(filePath, out string xmlString))
        {
            return false;
        }

        if (string.IsNullOrEmpty(xmlString))
        {
            return false;
        }

        try
        {
            document.LoadXml(xmlString);
        }
        catch
        {
            return false;
        }

        XmlNode? rootNode = document.DocumentElement;
        if (rootNode == null)
        {
            return false;
        }

        data = rootNode.ChildNodes;
        return true;
    }
}
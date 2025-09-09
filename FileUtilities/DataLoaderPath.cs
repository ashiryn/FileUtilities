using System;

namespace FluffyVoid.FileUtilities;

/// <summary>
///     Data path definition to retrieve files from default special folders based on the type of data location
/// </summary>
public class DataLoaderPath : IDataLoaderPath
{
    /// <summary>
    ///     Retrieves the data path to use for file IO
    /// </summary>
    /// <param name="dataLocation">The data location to get a file path for</param>
    /// <returns>The file path to use for file IO</returns>
    public string GetDataPath(DataLocation dataLocation)
    {
        return dataLocation switch
        {
            DataLocation.ApplicationData =>
                $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{AppDomain.CurrentDomain.FriendlyName}",
            DataLocation.UserData =>
                $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/{AppDomain.CurrentDomain.FriendlyName}",
            DataLocation.SaveData =>
                $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/{AppDomain.CurrentDomain.FriendlyName}/Saves",
            _ => "/Data"
        };
    }
}
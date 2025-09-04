namespace FluffyVoid.FileUtilities.DataLoader;

/// <summary>
///     Data path definition to retrieve files from a location alongside the running exectuable
/// </summary>
public class NextToExecutablePath : IDataLoaderPath
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
            DataLocation.ApplicationData => "/Data",
            DataLocation.UserData        => "/Data/User",
            DataLocation.SaveData        => "/Data/Saves",
            _                            => "/Data"
        };
    }
}
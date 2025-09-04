namespace FluffyVoid.FileUtilities.DataLoader;

/// <summary>
///     Data path definition to retrieve files from a debug location relative to the bin folder
///     Useful for debugging files that are stored in source control
/// </summary>
public class DebugPath : IDataLoaderPath
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
            DataLocation.ApplicationData => "../../Data/ApplicationData",
            DataLocation.UserData        => "../../Data/UserData",
            DataLocation.SaveData        => "../../Data/SaveData",
            _                            => "../../Data"
        };
    }
}
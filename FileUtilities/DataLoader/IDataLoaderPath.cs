namespace FluffyVoid.FileUtilities.DataLoader;

/// <summary>
///     Defines a contract for all data loaders to enforce functionality needed
/// </summary>
public interface IDataLoaderPath
{
    /// <summary>
    ///     Retrieves the data path to use for file IO
    /// </summary>
    /// <param name="dataLocation">The data location to get a file path for</param>
    /// <returns>The file path to use for file IO</returns>
    public string GetDataPath(DataLocation dataLocation);
}
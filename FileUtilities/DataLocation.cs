namespace FluffyVoid.FileUtilities;

/// <summary>
///     The types of data storage locations the data loader supports
/// </summary>
public enum DataLocation
{
    /// <summary>
    /// Location used to store application specific data and/or settings
    /// </summary>
    ApplicationData,
    /// <summary>
    /// Location used to store user specific data and/or settings
    /// </summary>
    UserData,
    /// <summary>
    /// Location used to store save data for an application session
    /// </summary>
    SaveData
}
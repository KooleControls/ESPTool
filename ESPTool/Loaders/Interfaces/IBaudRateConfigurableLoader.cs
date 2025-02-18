namespace ESPTool.Loaders.Interfaces
{
    /// <summary>
    /// Interface for loaders that support baud rate changes.
    /// </summary>
    public interface IBaudRateConfigurableLoader : ILoader
    {
        Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token);
    }
}

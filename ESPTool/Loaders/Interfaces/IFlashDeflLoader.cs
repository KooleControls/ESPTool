namespace ESPTool.Loaders.Interfaces
{
    /// <summary>
    /// Interface for loaders that support compressed flashing.
    /// </summary>
    public interface IFlashDeflLoader : ILoader
    {
        Task FlashDeflBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task FlashDeflDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task FlashDeflEndAsync(uint executeFlags, uint entryPoint, CancellationToken token);
    }
}

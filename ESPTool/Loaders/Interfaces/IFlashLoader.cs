namespace ESPTool.Loaders.Interfaces
{
    /// <summary>
    /// Interface for loaders that support flashing operations.
    /// </summary>
    public interface IFlashLoader : ILoader
    {
        Task FlashBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task FlashDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task FlashEndAsync(uint execute, uint entryPoint, CancellationToken token);
    }
}


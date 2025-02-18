namespace ESPTool.Loaders.Interfaces
{
    /// <summary>
    /// Interface for loaders that support in-memory operations.
    /// </summary>
    public interface IMemLoader : ILoader
    {
        Task MemBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task MemDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task MemEndAsync(uint execute, uint entryPoint, CancellationToken token);
    }
}

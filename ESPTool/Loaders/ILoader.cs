using System;
using System.Threading;
using System.Threading.Tasks;

namespace EspDotNet.Loaders
{
    /// <summary>
    /// Base interface for all loaders.
    /// </summary>
    public interface ILoader
    {
        Task<uint> ReadRegisterAsync(uint address, CancellationToken token);
        Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token);
        Task EraseFlashAsync(CancellationToken token);

        Task FlashDeflBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task FlashDeflDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task FlashDeflEndAsync(uint executeFlags, uint entryPoint, CancellationToken token);

        Task FlashBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task FlashDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task FlashEndAsync(uint execute, uint entryPoint, CancellationToken token);

        Task MemBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token);
        Task MemDataAsync(byte[] blockData, uint seq, CancellationToken token);
        Task MemEndAsync(uint execute, uint entryPoint, CancellationToken token);

    }
}

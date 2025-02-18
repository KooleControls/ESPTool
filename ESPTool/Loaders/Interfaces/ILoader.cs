using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders.Interfaces
{
    /// <summary>
    /// Base interface for all loaders.
    /// </summary>
    public interface ILoader
    {
        Task SyncAsync(CancellationToken token);
        Task<uint> ReadRegisterAsync(uint address, CancellationToken token);
    }
}

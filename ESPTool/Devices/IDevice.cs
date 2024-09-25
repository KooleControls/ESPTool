using ESPTool.Loaders;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESPTool.Devices
{

    public interface IDevice
    {
        public Task EraseFlashAsync(CancellationToken token = default);
        public Task UploadToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null);
        public Task UploadCompressedToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null);
        public Task ResetDeviceAsync(CancellationToken token = default);
    }





}

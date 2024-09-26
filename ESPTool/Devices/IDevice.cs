using ESPTool.Loaders;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESPTool.Devices
{

    public interface IDevice
    {
        public Task ResetDeviceAsync(CancellationToken token = default);
    }





}

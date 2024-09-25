using ESPTool.Com;
using ESPTool.Loaders;

namespace ESPTool.Devices
{
    public class ESP32Device : IDevice
    {
        private readonly SoftLoader _loader;
        private readonly Communicator _communicator;

        public ESP32Device(Communicator communicator)
        {
            _communicator = communicator;
            _loader = new SoftLoader(_communicator);
        }


        /// <summary>
        /// Erases the flash memory.
        /// </summary>
        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            await _loader.EraseFlashAsync(token);
        }


        public async Task UploadToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {
        }


        public async Task UploadCompressedToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {

        }

        public async Task ResetDeviceAsync(CancellationToken token = default)
        {
            await _communicator.ResetDeviceAsync(token);
        }
    }





}

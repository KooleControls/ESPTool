using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools.Firmware;

namespace EspDotNet.Tools
{
    public class SoftloaderTool
    {
        private readonly Communicator _communicator;
        private readonly UploadRamTool _uploadTool;
        private readonly IFirmwareProvider _firmwareProvider;

        public SoftloaderTool(Communicator communicator, UploadRamTool uploadTool, IFirmwareProvider firmwareProvider)
        {
            _communicator = communicator;
            _uploadTool = uploadTool;
            _firmwareProvider = firmwareProvider;
        }

        public async Task<SoftLoader> StartAsync(CancellationToken token = default)
        {
            // Upload the stubloader
            var firmwareTool = new FirmwareUploadTool(_uploadTool);
            await firmwareTool.UploadFirmwareAndExecuteAsync(_firmwareProvider, token);

            // Instantiate loader and synchronize
            SoftLoader softLoader = new SoftLoader(_communicator);
            await softLoader.WaitForOHAIAsync(token);
            return softLoader;
        }
    }
}

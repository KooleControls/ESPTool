using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools.Firmware;

namespace EspDotNet.Tools
{
    public class SoftLoaderTool
    {
        private readonly Communicator _communicator;
        private readonly IUploadTool _uploadTool;

        public SoftLoaderTool(Communicator communicator, IUploadTool uploadTool)
        {
            _communicator = communicator;
            _uploadTool = uploadTool;
        }

        public async Task<SoftLoader> StartAsync(IFirmwareProvider firmwareProvider, CancellationToken token = default)
        {
            // Upload the StubLoader
            var firmwareTool = new FirmwareUploadTool(_uploadTool);
            await firmwareTool.UploadFirmwareAndExecuteAsync(firmwareProvider, token);

            // Instantiate loader and synchronize
            SoftLoader softLoader = new SoftLoader(_communicator);
            await softLoader.WaitForOHAIAsync(token);
            return softLoader;
        }
    }
}

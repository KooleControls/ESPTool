using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools;
using EspDotNet.Tools.Firmware;
using System.IO.Ports;

namespace EspDotNet
{
    public class ESPToolbox
    {
        public ESPToolConfig Config { get; private set; }
        public ESPToolbox() => Config = ConfigProvider.LoadDefaultConfig();
        public ESPToolbox(ESPToolConfig config) => Config = config;


        public Communicator CreateCommunicator(SerialPort port) => new Communicator(port);


        public BootloaderTool CreateBootloaderTool(Communicator communicator) =>
            new BootloaderTool(communicator, Config.BootloaderSequence);

        public SoftLoaderTool CreateSoftLoaderTool(Communicator communicator, IUploadTool uploadTool) =>
            new SoftLoaderTool(communicator, uploadTool);

        public RamUploadTool CreateRamUploadTool(ILoader loader, DeviceConfig deviceConfig) =>
            new RamUploadTool(loader, deviceConfig);

        public ChangeBaudRateTool CreateChangeBaudRateTool(ILoader loader) =>
            new ChangeBaudRateTool(loader);

        public FlashDownloadTool CreateReadFlashTool(Communicator communicator, SoftLoader loader) =>
            new FlashDownloadTool(loader, communicator);

        public FlashUploadTool CreateUploadFlashTool(ILoader loader, DeviceConfig deviceConfig) =>
            new FlashUploadTool(loader, deviceConfig);

        public FlashUploadDeflatedTool CreateUploadFlashDeflatedTool(SoftLoader loader, DeviceConfig deviceConfig) =>
            new FlashUploadDeflatedTool(loader, deviceConfig);

        public FlashEraseTool CreateEraseTool(SoftLoader loader) =>
            new FlashEraseTool(loader);

        public ChipTypeDetectTool CreateChipTypeDetectTool(ILoader loader) =>
            new ChipTypeDetectTool(loader, Config);

        public EFuseTool CreateEFuseTool(ILoader loader, DeviceConfig deviceConfig) =>
            new EFuseTool(loader, deviceConfig);

        public ResetDeviceTool CreateResetTool(Communicator communicator) =>
            new ResetDeviceTool(communicator, Config.ResetSequence ?? throw new InvalidOperationException("No reset sequence"));

        public FirmwareUploadTool CreateFirmwareUploadTool(IUploadTool tool) =>
            new FirmwareUploadTool(tool);

    }
}

using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Loaders.ESP32BootLoader;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools;
using EspDotNet.Tools.Firmware;
using System.Text.Json;

namespace EspDotNet
{
    public class ESPTool
    {
        private readonly Communicator _communicator;
        private readonly ESPToolConfig _config;

        public ESPTool()
        {
            _communicator = new Communicator();
            _config = ConfigProvider.LoadDefaultConfig();
        }

        public ESPTool(ESPToolConfig config)
        {
            _communicator = new Communicator();
            _config = config;
        }

        public void OpenSerial(string portName, int baudRate) => _communicator.OpenSerial(portName, baudRate);
        public void CloseSerial() => _communicator.CloseSerial();

        // --------
        // TOOL BOX
        // --------
        public BootloaderTool GetBootloaderTool()
        {
            return new BootloaderTool(_communicator, _config.BootloaderSequence);
        }

        public SoftloaderTool GetSoftloaderTool(ILoader loader, ChipTypes chipType)
        {
            var firmwareProvider = DefaultFirmwareProviders.GetSoftloaderForDevice(chipType);
            var uploadTool = GetUploadRamTool(loader, chipType);
            return new SoftloaderTool(_communicator, uploadTool, firmwareProvider);
        }

        public ResetDeviceTool GetResetDeviceTool()
        {
            var resetSequence = _config.ResetSequence ?? throw new Exception("Config error, no reset sequence found");
            return new ResetDeviceTool(_communicator, resetSequence);
        }

        public ChipTypeDetectTool GetChipDetectTool(ILoader loader)
        {
            return new ChipTypeDetectTool(loader, _config);
        }

        public UploadRamTool GetUploadRamTool(ILoader loader, ChipTypes chipType)
        {
            var deviceConfig = _config.Devices.FirstOrDefault(deviceConfig => deviceConfig.ChipType == chipType) ?? throw new Exception($"No config found for device {chipType}");
            var tool = new UploadRamTool(loader) {
                BlockSize = (UInt32)deviceConfig.FlashBlockSize
            };
            return tool;
        }

        public UploadFlashTool GetUploadFlashTool(ILoader loader, ChipTypes chipType)
        {
            var deviceConfig = _config.Devices.FirstOrDefault(deviceConfig => deviceConfig.ChipType == chipType) ?? throw new Exception($"No config found for device {chipType}");
            var tool = new UploadFlashTool(loader) {
                BlockSize = (UInt32)deviceConfig.FlashBlockSize
            };
            return tool;
        }

        public UploadFlashDeflatedTool GetUploadFlashDeflatedTool(SoftLoader loader, ChipTypes chipType)
        {
            var deviceConfig = _config.Devices.FirstOrDefault(deviceConfig => deviceConfig.ChipType == chipType) ?? throw new Exception($"No config found for device {chipType}");
            var tool = new UploadFlashDeflatedTool(loader) {
                BlockSize = (UInt32)deviceConfig.FlashBlockSize
            };
            return tool;
        }

        public FirmwareUploadTool GetFirmwareUploadTool(IUploadTool uploadTool)
        {
            return new FirmwareUploadTool(uploadTool);
        }

        public ChangeBaudrateTool GetChangeBaudrateTool(ILoader loader)
        {
            return new ChangeBaudrateTool(_communicator, loader);
        }

        public FlashEraseTool GetFlashEraseTool(SoftLoader loader)
        {
            return new FlashEraseTool(loader);
        }

        public EFuseTool GetEFuseTool(ILoader loader, ChipTypes chipType)
        {
            var deviceConfig = _config.Devices.FirstOrDefault(deviceConfig => deviceConfig.ChipType == chipType) ?? throw new Exception($"No config found for device {chipType}");
            return new EFuseTool(loader, deviceConfig);
        }
    }
}

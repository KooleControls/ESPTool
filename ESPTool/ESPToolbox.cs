using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Loaders;
using EspDotNet.Tools.Firmware;
using EspDotNet.Tools;
using Microsoft.Extensions.Logging;

namespace EspDotNet
{
    public class ESPToolbox
    {
        private readonly ESPToolConfig _config;

        public ESPToolbox()
        {
            _config = ConfigProvider.LoadDefaultConfig();
        }

        public ESPToolbox(ESPToolConfig config)
        {
            _config = config;
        }

        public Communicator CreateCommunicator()
        {
            return new Communicator();
        }

        public void OpenSerial(Communicator communicator, string portName, int baudRate)
        {
            communicator.OpenSerial(portName, baudRate);
        }

        public void CloseSerial(Communicator communicator)
        {
            communicator.CloseSerial();
        }

        public async Task<ILoader> StartBootloaderAsync(Communicator communicator, CancellationToken token = default)
        {
            var bootloader = new BootloaderTool(communicator, _config.BootloaderSequence);
            return await bootloader.StartAsync(token);
        }

        public async Task<ChipTypes> DetectChipTypeAsync(ILoader loader, CancellationToken token = default)
        {
            var detectTool = new ChipTypeDetectTool(loader, _config);
            return await detectTool.DetectChipTypeAsync(token);
        }

        public async Task<SoftLoader> StartSoftloaderAsync(
            Communicator communicator,
            ILoader bootloader,
            ChipTypes chipType,
            CancellationToken token = default)
        {
            var deviceConfig = GetDeviceConfig(chipType);
            var firmwareProvider = DefaultFirmwareProviders.GetSoftloaderForDevice(chipType);
            var uploadTool = new UploadRamTool(bootloader)
            {
                BlockSize = (uint)deviceConfig.FlashBlockSize
            };
            var softloaderTool = new SoftloaderTool(communicator, uploadTool, firmwareProvider);
            return await softloaderTool.StartAsync(token);
        }

        public async Task ChangeBaudAsync(Communicator communicator, ILoader loader, int baudRate, CancellationToken token = default)
        {
            var baudTool = new ChangeBaudrateTool(communicator, loader);
            await baudTool.ChangeBaudAsync(baudRate, token);
        }

        public async Task EraseFlashAsync(SoftLoader loader, CancellationToken token = default)
        {
            var eraseTool = new FlashEraseTool(loader);
            await eraseTool.EraseFlashAsync(token);
        }

        public async Task ResetDeviceAsync(Communicator communicator, CancellationToken token = default)
        {
            var sequence = _config.ResetSequence ?? throw new InvalidOperationException("No reset sequence in config");
            var resetTool = new ResetDeviceTool(communicator, sequence);
            await resetTool.ResetAsync(token);
        }

        public async Task<byte[]> ReadEfuseAsync(ILoader loader, ChipTypes chipType, EFlagKey key, CancellationToken token = default)
        {
            var tool = new EFuseTool(loader, GetDeviceConfig(chipType));
            return await tool.ReadAsync(key, token);
        }

        public ReadFlashTool CreateReadFlashTool(Communicator communicator, SoftLoader softLoader, ChipTypes chipType)
        {
            var deviceConfig = GetDeviceConfig(chipType);
            return new ReadFlashTool(softLoader, communicator);
        }

        public IUploadTool CreateUploadRamTool(ILoader loader, ChipTypes chipType)
        {
            return new UploadRamTool(loader)
            {
                BlockSize = (uint)GetDeviceConfig(chipType).FlashBlockSize
            };
        }

        public IUploadTool CreateUploadFlashTool(ILoader loader, ChipTypes chipType)
        {
            return new UploadFlashTool(loader)
            {
                BlockSize = (uint)GetDeviceConfig(chipType).FlashBlockSize
            };
        }

        public IUploadTool CreateUploadFlashDeflatedTool(SoftLoader loader, ChipTypes chipType)
        {
            return new UploadFlashDeflatedTool(loader)
            {
                BlockSize = (uint)GetDeviceConfig(chipType).FlashBlockSize
            };
        }

        public async Task UploadFirmwareAsync(
            IUploadTool uploadTool,
            IFirmwareProvider firmware,
            CancellationToken token = default,
            IProgress<float>? progress = null)
        {
            var firmwareTool = new FirmwareUploadTool(uploadTool)
            {
                Progress = progress ?? new Progress<float>()
            };

            await firmwareTool.UploadFirmwareAsync(firmware, token);
        }

        public async Task UploadFirmwareAndExecuteAsync(
            IUploadTool uploadTool,
            IFirmwareProvider firmware,
            Communicator communicator,
            CancellationToken token = default,
            IProgress<float>? progress = null)
        {
            await UploadFirmwareAsync(uploadTool, firmware, token, progress);
            await ResetDeviceAsync(communicator, token);
        }

        private DeviceConfig GetDeviceConfig(ChipTypes chipType)
        {
            return _config.Devices.FirstOrDefault(d => d.ChipType == chipType)
                ?? throw new InvalidOperationException($"No device config found for {chipType}");
        }
    }
}

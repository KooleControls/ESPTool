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
        private ILoader? _loader;

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


        public async Task StartBootloaderAsync(CancellationToken token = default)
        {
            // Enter the device bootloader
            var bootloaderSequence = _config.BootloaderSequence ?? throw new Exception("Config error, no bootloader sequence found");
            await _communicator.ExecutePinSequence(bootloaderSequence, token);

            // Create bootloader instance
            var bootloader = new ESP32BootLoader(_communicator);
            if (!await bootloader.Synchronize(token))
                throw new Exception("Failed to synchronize with bootloader");

            _loader = bootloader;
        }

        public async Task StartSoftloaderAsync(IFirmwareProvider softloader, CancellationToken token = default)
        {
            if (_loader == null)
                throw new Exception("No loader available, start bootloader first");

            // Detect chip type
            var chipType = await DetectChipTypeAsync(token);

            // Upload the softstarter
            await UploadFirmwareAndExecuteAsync(softloader, FirmwareUploadMethods.Ram, token);

            // Wait for the softloader to start
            SoftLoader softLoader = new SoftLoader(_communicator);
            await softLoader.WaitForOHAIAsync(token);

            _loader = softLoader;
        }




        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            if (_loader == null)
                throw new Exception("No loader available, start loader first");
            FlashEraseTool flashEraser = new FlashEraseTool(_loader);
            await flashEraser.EraseFlashAsync(token);
        }

        public async Task UploadFirmwareAsync(IFirmwareProvider firmware, FirmwareUploadMethods method, CancellationToken token = default, IProgress<float>? progress = default)
        {
            if (_loader == null)
                throw new Exception("No loader available, start loader first");

            // Detect chip type
            var chipType = await DetectChipTypeAsync(token);

            // Upload the softstarter
            var uploadConfig = GetFirmwareUploadConfig(chipType, method, false);
            FirmwareUploadTool firmwareSender = new FirmwareUploadTool(_loader, uploadConfig);
            firmwareSender.Progress = progress ?? new Progress<float>();
            await firmwareSender.UploadFirmwareAsync(firmware, token);
        }

        public async Task UploadFirmwareAndExecuteAsync(IFirmwareProvider firmware, FirmwareUploadMethods method, CancellationToken token = default, IProgress<float>? progress = default)
        {
            if (_loader == null)
                throw new Exception("No loader available, start loader first");

            // Detect chip type
            var chipType = await DetectChipTypeAsync(token);

            // Upload the softstarter
            var uploadConfig = GetFirmwareUploadConfig(chipType, method, true);
            FirmwareUploadTool firmwareSender = new FirmwareUploadTool(_loader, uploadConfig);
            firmwareSender.Progress = progress ?? new Progress<float>();
            await firmwareSender.UploadFirmwareAsync(firmware, token);
        }

        public async Task<ChipTypes> DetectChipTypeAsync(CancellationToken token = default)
        {
            if (_loader == null)
                throw new Exception("No loader available, start loader first");
            ChipTypeDetectTool chipTypeDetector = new ChipTypeDetectTool(_loader);
            return await chipTypeDetector.DetectChipTypeAsync(token);
        }

        public async Task ResetDeviceAsync(CancellationToken token = default)
        {
            var bootloaderSequence = _config.ResetSequence ?? throw new Exception("Config error, no reset sequence found");
            await _communicator.ExecutePinSequence(bootloaderSequence, token);
        }

        public async Task ChangeBaudAsync(int baud, CancellationToken token)
        {
            var oldBaud = _communicator.GetBaudRate();
            if (baud == oldBaud)
                return;
            if (_loader == null)
                throw new Exception("No loader available, start loader first");
            ChangeBaudrateTool changeBaudrateTool = new ChangeBaudrateTool(_loader);
            await changeBaudrateTool.ChangeBaudAsync(baud, oldBaud, token);
            _communicator.ChangeBaudRate(baud);
        }



        private FirmwareUploadConfig GetFirmwareUploadConfig(ChipTypes chipType, FirmwareUploadMethods uploadMethod, bool execute)
        {
            // Find device information
            var deviceInfo = _config.Devices.FirstOrDefault(fw => fw.ChipType == chipType) ?? throw new Exception($"No deviceconfig found for '{chipType}'");
            return new FirmwareUploadConfig
            {
                BlockSize = uploadMethod == FirmwareUploadMethods.Ram ? (uint)deviceInfo.RamBlockSize : (uint)deviceInfo.FlashBlockSize,
                ExecuteAfterSending = execute,
                UploadMethod = uploadMethod
            };
        }
    }
}

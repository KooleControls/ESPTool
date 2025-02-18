using ESPTool.Communication;
using ESPTool.Loaders;
using ESPTool.Loaders.ESP32BootLoader;
using ESPTool.Loaders.SoftLoader;
using ESPTool.Models;
using ESPTool.Tools;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ESPTool
{
    public class DeviceManager
    {
        private readonly Communicator _communicator;
        private SoftLoader? _softLoader;
        public AppConfig AppConfig { get; set; }
        public DeviceManager(ILoggerFactory loggerFactory)
        {
            _communicator = new Communicator();

            string configJSON = File.ReadAllText("AppConfig.json");
            AppConfig = JsonSerializer.Deserialize<AppConfig>(configJSON) ?? new();
        }

        public void OpenSerial(string portName, int baudRate) => _communicator.OpenSerial(portName, baudRate);
        public void CloseSerial() => _communicator.CloseSerial();

        public async Task<ESP32BootLoader> StartBootloader(CancellationToken token = default)
        {
            var bootloaderSequence = AppConfig.BootloaderSequence ?? throw new Exception("Config error, no bootloader sequence found");
            await _communicator.ExecutePinSequence(bootloaderSequence, token);
            var bootloader = new ESP32BootLoader(_communicator);
            var success = await bootloader.Synchronize(token);

            if (!success)
                throw new Exception("Failed to synchronize with bootloader");

            return bootloader;
        }

        public async Task<SoftLoader> StartSoftloader(ILoader loader, CancellationToken token = default)
        {
            var chipType = await DetectChipTypeAsync(loader, token);
            var deviceInfo = AppConfig.Devices.FirstOrDefault(fw => fw.ChipType == chipType) ?? throw new Exception($"No deviceconfig found for '{chipType}'");
            var firmware = AppConfig.SoftLoaders.FirstOrDefault(fw => fw.ChipType == chipType) ?? throw new Exception($"No softloader found for '{chipType}'");

            FirmwareSender firmwareSender = new FirmwareSender(loader);
            firmwareSender.BlockSize = deviceInfo.RamBlockSize;
            firmwareSender.ExecuteAfterSending = true;
            await firmwareSender.UploadFirmwareAsync(firmware);

            SoftLoader softLoader = new SoftLoader(_communicator);
            await softLoader.WaitForOHAIAsync(token);

            return softLoader;
        }

        private async Task<ChipTypes> DetectChipTypeAsync(ILoader loader, CancellationToken token)
        {
            uint CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; // This ROM address has a different value on each chip model
            uint registerValue = await loader.ReadRegisterAsync(CHIP_DETECT_MAGIC_REG_ADDR, token);
            return (ChipTypes)registerValue;
        }
    }
}

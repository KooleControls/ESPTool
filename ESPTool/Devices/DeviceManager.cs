using ESPTool.Com;
using ESPTool.Firmware;
using ESPTool.Loaders;

namespace ESPTool.Devices
{
    public class DeviceManager
    {
        private readonly Loader _loader;
        private readonly Communicator _communicator;

        public DeviceManager()
        {
            _communicator = new Communicator();
            _loader = new Loader(_communicator);
        }

        /// <summary>
        /// Initializes the device by entering the bootloader, syncing, detecting the chip, uploading the softloader, and changing the baud rate.
        /// </summary>
        public async Task<IDevice> InitializeAsync(string comPort, int baudRate, CancellationToken token = default)
        {
            // 1. Open the serial port
            _communicator.OpenSerial(comPort, baudRate);

            // 2. Enter the bootloader
            await _communicator.EnterBootloaderAsync(token);

            // 3. Sync with the bootloader
            await SyncBootloader(token);

            // 4. Detect the chip type
            var chipType = await DetectChipTypeAsync(token);

            switch (chipType)
            {
                case ChipTypes.ESP32:
                {
                    UInt32 ESP_RAM_BLOCK = 0x1800;
                    UInt32 FLASH_WRITE_SIZE = 0x400;
                    SoftLoaderFlasher flasher = new SoftLoaderFlasher(_loader);
                    await flasher.FlashSoftLoaderAsync(ESP_RAM_BLOCK, token);
                    await _loader.ChangeBaudAsync(115200, baudRate, token);
                    return new ESP32Device(_communicator);
                }
                default:
                    throw new NotImplementedException($"Chiptype {chipType} not implemented ");
            }
        }

        private async Task<ChipTypes> DetectChipTypeAsync(CancellationToken token)
        {
            uint CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; // This ROM address has a different value on each chip model
            uint registerValue = await _loader.ReadRegisterAsync(CHIP_DETECT_MAGIC_REG_ADDR, token);
            return (ChipTypes)registerValue;
        }

        /// <summary>
        /// Synchronizes with the bootloader.
        /// </summary>
        private async Task SyncBootloader(CancellationToken token)
        {
            await _loader.SyncAsync(token);
        }
    }
}

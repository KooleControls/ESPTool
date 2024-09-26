using ESPTool.Loaders;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ESPTool.Communication;

namespace ESPTool.Devices
{
    public class DeviceManager
    {
        private readonly Loader _loader;
        private readonly Communicator _communicator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DeviceManager> _logger;

        public DeviceManager(ILoggerFactory loggerFactory)
        {
            _communicator = new Communicator();
            _loader = new Loader(_communicator);
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<DeviceManager>();
        }

        

        /// <summary>
        /// Initializes the device by entering the bootloader, syncing, detecting the chip, uploading the softloader, and changing the baud rate.
        /// </summary>
        public async Task<IDevice> InitializeAsync(string comPort, int baudRate, CancellationToken token = default)
        {
            // 1. Open the serial port
            _communicator.OpenSerial(comPort, baudRate);
            _logger.LogInformation("Opened serial port {ComPort}", comPort);

            // 2. Enter the bootloader
            _logger.LogInformation("Execute bootloader sequence...");
            await _communicator.EnterBootloaderAsync(token);

            // 3. Sync with the bootloader
            _logger.LogInformation("Syncing with bootloader...");
            await SyncBootloader(token);

            // 4. Detect the chip type
            _logger.LogInformation("Detecting chip type...");
            var chipType = await DetectChipTypeAsync(token);
            _logger.LogInformation("Detected chip type: {ChipType}", chipType);



            switch (chipType)
            {
                case ChipTypes.ESP32:
                    _logger.LogInformation("Initializing ESP32 device...");
                    return new ESP32Device(_communicator, _loggerFactory);

                default:
                    _logger.LogError("Chip type {ChipType} not implemented.", chipType);
                    throw new NotImplementedException($"Chip type {chipType} not implemented");
            }
        }

        private async Task<ChipTypes> DetectChipTypeAsync(CancellationToken token)
        {
            uint CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; // This ROM address has a different value on each chip model
            uint registerValue = await _loader.ReadRegisterAsync(CHIP_DETECT_MAGIC_REG_ADDR, token);
            _logger.LogInformation("Chip detect register value: {RegisterValue:X}", registerValue);
            return (ChipTypes)registerValue;
        }

        /// <summary>
        /// Synchronizes with the bootloader.
        /// </summary>
        private async Task SyncBootloader(CancellationToken token)
        {
            for (int tryNo = 0; tryNo < 100; tryNo++)
            {
                token.ThrowIfCancellationRequested();

                // Try to sync for 100ms.
                using CancellationTokenSource cts = new CancellationTokenSource() ;

                // Register the token and store the registration to dispose of it later
                using CancellationTokenRegistration ctr = token.Register(() => cts.Cancel());
                    
                cts.CancelAfter(100); // Cancel after 100ms

                try
                {
                    _logger.LogInformation("Attempting to sync with bootloader (Attempt {TryNo})...", tryNo + 1);
                    await _loader.SyncAsync(cts.Token);
                    _logger.LogInformation("Bootloader sync successful on attempt {TryNo}.", tryNo + 1);
                    return; // Sync succeeded
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Sync attempt {TryNo} timed out.", tryNo + 1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during bootloader sync on attempt {TryNo}.", tryNo + 1);
                    throw;
                }
                finally
                {
                    ctr.Unregister();
                }
            }

            _logger.LogError("Failed to synchronize with the bootloader after 100 attempts.");
            throw new Exception("Couldn't synchronize after 100 attempts");
        }

    }
}

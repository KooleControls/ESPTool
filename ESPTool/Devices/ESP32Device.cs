using ESPTool.Communication;
using ESPTool.Flashers;
using ESPTool.Loaders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Devices
{
    public class ESP32Device : IDevice
    {
        private readonly UInt32 ESP_RAM_BLOCK = 0x1800;
        private readonly UInt32 FLASH_WRITE_SIZE = 0x400;
        private readonly Communicator _communicator;
        private Loader _loader;
        private SoftLoader? _softLoader;
        private readonly ILogger<ESP32Device> _logger;

        public ESP32Device(Communicator communicator, ILoggerFactory loggerFactory)
        {
            _communicator = communicator;
            _loader = new Loader(_communicator);
            _logger = loggerFactory.CreateLogger<ESP32Device>();
            _logger.LogInformation("ESP32Device instance created.");
        }

        /// <summary>
        /// Starts the softloader and flashes it to RAM.
        /// </summary>
        public async Task StartSoftloaderAsync(CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation("Starting softloader...");

                SoftLoaderFlasher flasher = new SoftLoaderFlasher(_loader);
                await flasher.FlashSoftLoaderAsync(ESP_RAM_BLOCK, token);

                // Instantiate the softloader
                _softLoader = new SoftLoader(_communicator);
                _logger.LogInformation("Softloader started and flashed to RAM.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start the softloader.");
                throw;
            }
        }

        /// <summary>
        /// Changes the baud rate using the softloader.
        /// </summary>
        public async Task ChangeBaudAsync(int baud, CancellationToken token = default)
        {
            try
            {
                if (_softLoader == null)
                {
                    _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                    throw new InvalidOperationException("Not supported by default loader, start the softloader first");
                }

                _logger.LogInformation("Changing baud rate to {BaudRate}...", baud);
                await _softLoader.ChangeBaudAsync(115200, baud, token);
                _communicator.ChangeBaudRate(baud);
                _logger.LogInformation("Baud rate changed to {BaudRate}.", baud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change baud rate to {BaudRate}.", baud);
                throw;
            }
        }

        /// <summary>
        /// Erases the flash using the softloader.
        /// </summary>
        public async Task EraseFlashAsync(CancellationToken token = default)
        {
            try
            {
                if (_softLoader == null)
                {
                    _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                    throw new InvalidOperationException("Not supported by default loader, start the softloader first");
                }

                _logger.LogInformation("Erasing flash...");
                await _softLoader.EraseFlashAsync(token);
                _logger.LogInformation("Flash erased successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to erase flash.");
                throw;
            }
        }

        /// <summary>
        /// Uploads data to flash without compression.
        /// </summary>
        public async Task UploadToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {
            try
            {
                if (_softLoader == null)
                {
                    _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                    throw new InvalidOperationException("Not supported by default loader, start the softloader first");
                }

                _logger.LogInformation("Uploading data to flash at offset {Offset:X}...", offset);

                // Logic for uploading data goes here...
                throw new NotImplementedException("UploadToFlashAsync method is not implemented yet.");

                // Log completion
                _logger.LogInformation("Upload to flash at offset {Offset:X} completed.", offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload data to flash at offset {Offset:X}.", offset);
                throw;
            }
        }

        /// <summary>
        /// Uploads compressed data to flash.
        /// </summary>
        public async Task UploadCompressedToFlashAsync(Stream data, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {
            try
            {
                if (_softLoader == null)
                {
                    _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                    throw new InvalidOperationException("Not supported by default loader, start the softloader first");
                }

                _logger.LogInformation("Uploading compressed data to flash at offset {Offset:X}...", offset);

                // Logic for uploading compressed data goes here...
                throw new NotImplementedException("UploadCompressedToFlashAsync method is not implemented yet.");

                // Log completion
                _logger.LogInformation("Compressed upload to flash at offset {Offset:X} completed.", offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload compressed data to flash at offset {Offset:X}.", offset);
                throw;
            }
        }

        /// <summary>
        /// Resets the device.
        /// </summary>
        public async Task ResetDeviceAsync(CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation("Resetting device...");
                await _communicator.ResetDeviceAsync(token);
                _logger.LogInformation("Device reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset the device.");
                throw;
            }
        }
    }
}

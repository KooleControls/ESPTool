using ESPTool.Communication;
using ESPTool.Flashers;
using ESPTool.Loaders;
using ESPTool.Utils;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing;

namespace ESPTool.Devices
{
    public class ESP32Device : IDevice
    {
        private readonly UInt32 ESP_RAM_BLOCK = 0x1800;
        private readonly UInt32 FLASH_WRITE_SIZE = 0x400;
        private readonly Communicator _communicator;
        private ESP32Loader _loader;
        private SoftLoader? _softLoader;
        private readonly ILogger<ESP32Device> _logger;

        public ESP32Device(Communicator communicator, ILoggerFactory loggerFactory)
        {
            _communicator = communicator;
            _loader = new ESP32Loader(_communicator);
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
                _logger.LogInformation("Changing baud rate to {BaudRate}...", baud);

                if (_softLoader != null)
                {
                    await _softLoader.ChangeBaudAsync(115200, baud, token);
                    _communicator.ChangeBaudRate(baud);
                }
                else
                {
                    await _loader.ChangeBaudAsync(baud, token);
                    _communicator.ChangeBaudRate(baud);
                }

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
        /// Uploads uncompressed data to flash.
        /// </summary>
        public async Task UploadToFlashAsync(Stream stream, UInt32 totalSize, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {
            UInt32 blockSize = (UInt32)FLASH_WRITE_SIZE;
            UInt32 blocks = totalSize / blockSize;
            if (totalSize % blockSize != 0)
                blocks++;

            float uploadedSize = 0;

            _logger.LogInformation("Starting firmware upload to flash at offset 0x{Offset:X}, size: {Size}.", offset, totalSize);


            if (_softLoader != null)
                await _softLoader.FlashBeginAsync(totalSize, blocks, blockSize, offset, token);
            else
                await _loader.FlashBeginAsync(totalSize, blocks, blockSize, offset, token);


            // Upload data blocks
            for (uint i = 0; i < blocks; i++)
            {
                token.ThrowIfCancellationRequested();
                UInt32 srcInd = i * blockSize;
                UInt32 len = (UInt32)totalSize - srcInd;
                if (len > blockSize)
                    len = blockSize;

                byte[] buffer = new byte[len];
                int bytesRead = await stream.ReadAsync(buffer, 0, (int)len, token);

                if (bytesRead != len)
                {
                    _logger.LogError("Failed to read the full block from the stream.");
                    break;
                }

                if (_softLoader != null)
                    await _softLoader.FlashDataAsync(buffer, i, token);
                else
                    await _loader.FlashDataAsync(buffer, i, token);

                uploadedSize += len;
                progress?.Report(uploadedSize / totalSize);
            }

            _logger.LogInformation("Firmware upload to flash at offset 0x{Offset:X} completed.", offset);
        }

        public async Task UploadToFlashFinishAsync(bool execute, UInt32 entrypoint, CancellationToken token = default)
        {
            if (_softLoader != null)
                await _softLoader.FlashEndAsync((UInt32)(execute ? 0 : 1), entrypoint, token);
            else
                await _loader.FlashEndAsync((UInt32)(execute ? 0 : 1), entrypoint, token);
        }

        /// <summary>
        /// Uploads compressed data to flash.
        /// </summary>
        public async Task UploadCompressedToFlashAsync(Stream uncompressedStream, UInt32 uncompressedSize, uint offset, CancellationToken token = default, IProgress<float> progress = null)
        {
            if (_softLoader == null)
            {
                _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                throw new InvalidOperationException("Not supported by default loader, start the softloader first");
            }

            MemoryStream compressedStream = new MemoryStream();
            ZlibCompressionHelper.CompressToZlibStream(uncompressedStream, compressedStream);
            compressedStream.Position = 0;

            UInt32 compressedSize = (UInt32)compressedStream.Length;
            UInt32 blockSize = (UInt32)FLASH_WRITE_SIZE;
            UInt32 blocks = compressedSize / blockSize;
            if (compressedSize % blockSize != 0)
                blocks++;

            float uploadedSize = 0;

            _logger.LogInformation("Starting compressed firmware upload to flash at offset 0x{Offset:X}, size: {Size}.", offset, uncompressedSize);
            await _softLoader.FlashDeflBeginAsync(uncompressedSize, blocks, FLASH_WRITE_SIZE, offset, token);


            // Upload data blocks
            for (uint i = 0; i < blocks; i++)
            {
                token.ThrowIfCancellationRequested();
                uint srcInd = i * FLASH_WRITE_SIZE;
                uint len = Math.Min(FLASH_WRITE_SIZE, compressedSize - srcInd);

                byte[] buffer = new byte[len];
                int bytesRead = await compressedStream.ReadAsync(buffer, 0, (int)len, token);

                if (bytesRead != len)
                {
                    _logger.LogError("Failed to read the full block from the stream.");
                    break;
                }

                await _softLoader.FlashDeflDataAsync(buffer, i, token);

                uploadedSize += len;
                progress?.Report(uploadedSize / compressedSize);
            }

            _logger.LogInformation("Compressed firmware upload to flash at offset 0x{Offset:X} completed.", offset);
        }

        public async Task UploadCompressedToFlashFinishAsync(bool execute, UInt32 entrypoint, CancellationToken token = default)
        {
            if (_softLoader != null)
                await _softLoader.FlashDeflEndAsync((UInt32)(execute ? 0 : 1), entrypoint, token);
            else
            {
                _logger.LogError("Softloader is not running. Call StartSoftloaderAsync first.");
                throw new InvalidOperationException("Not supported by default loader, start the softloader first");
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

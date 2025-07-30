using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Tools.Firmware;
using EspDotNet.Utils;

namespace EspDotNet.Tools
{
    public class FlashUploadTool : IUploadTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();
        public uint BlockSize { get; set; } = 1024;
        private readonly ILoader _loader;
        private readonly DeviceConfig _deviceConfig;

        public FlashUploadTool(ILoader loader, DeviceConfig deviceConfig)
        {
            _loader = loader;
            _deviceConfig = deviceConfig;
        }

        public async Task Upload(Stream data, uint offset, uint size, CancellationToken token)
        {
            // Calculate blocks
            uint blocks = (size + BlockSize - 1) / BlockSize;
            await _loader.FlashBeginAsync(size, blocks, BlockSize, offset, token);

            // Send data
            for (uint i = 0; i < blocks; i++)
            {
                uint srcIndex = i * BlockSize;
                uint len = Math.Min(BlockSize, size - srcIndex);

                byte[] buffer = new byte[len];
                int bytesRead = await data.ReadAsync(buffer, 0, (int)len, token);
                if (bytesRead != len)
                    break;

                await _loader.FlashDataAsync(buffer, i, token);
                Progress.Report((float)(i + 1) / blocks);
            }

            // Sending end is not required here
            // https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html#writing-data
        }

        public async Task UploadAndExecute(Stream uncompressedData, uint offset, uint unCompressedSize, uint entryPoint, CancellationToken token)
        {
            await Upload(uncompressedData, offset, unCompressedSize, token);

            // End memory transfer, 0 means execute, confusing
            await _loader.FlashEndAsync(0, entryPoint, token);
        }
    }
}

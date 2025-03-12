using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools.Firmware;
using EspDotNet.Utils;
using System;

namespace EspDotNet.Tools
{
    public class UploadFlashDeflatedTool : IUploadTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();
        public uint BlockSize { get; set; } = 1024;
        private readonly SoftLoader _loader;

        public UploadFlashDeflatedTool(SoftLoader loader)
        {
            _loader = loader;
        }

        public async Task Upload(Stream uncompressedStream, uint offset, uint unCompressedSize, CancellationToken token)
        {
            MemoryStream compressedStream = new MemoryStream();
            ZlibCompressionHelper.CompressToZlibStream(uncompressedStream, compressedStream);
            compressedStream.Position = 0;

            UInt32 compressedSize = (UInt32)compressedStream.Length;
            UInt32 blockSize = (UInt32)BlockSize;
            UInt32 blocks = compressedSize / blockSize;
            if (compressedSize % blockSize != 0)
                blocks++;


            await _loader.FlashDeflBeginAsync(unCompressedSize, blocks, BlockSize, offset, token);

            // Send data
            for (uint i = 0; i < blocks; i++)
            {
                uint srcIndex = i * BlockSize;
                uint len = Math.Min(BlockSize, compressedSize - srcIndex);

                byte[] buffer = new byte[len];
                int bytesRead = await compressedStream.ReadAsync(buffer, 0, (int)len, token);
                if (bytesRead != len)
                    break;

                await _loader.FlashDeflDataAsync(buffer, i, token);
                Progress.Report((float)(i + 1) / blocks);
            }

            // Sending end is not required here
            // https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html#writing-data
        }

        public async Task UploadAndExecute(Stream uncompressedData, uint offset, uint unCompressedSize, uint entryPoint, CancellationToken token)
        {
            await Upload(uncompressedData, offset, unCompressedSize, token);

            // End memory transfer, 0 means execute, confusing
            await _loader.FlashDeflEndAsync(0, entryPoint, token);
        }
    }
}

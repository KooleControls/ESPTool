using ESPTool.Loaders;
using ESPTool.Loaders.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESPTool.Firmware
{

    public abstract class FirmwareSender
    {
        public uint BlockSize { get; set; } = 1024;
        public async Task FlashSoftLoaderAsync(Firmware firmware, CancellationToken token = default)
        {

            foreach(var segment in firmware.Segments)
            {
                var data = new MemoryStream(segment.Data);
                await UploadAsync(data, (uint)data.Length, segment.Offset, BlockSize, token);
            }

            // Upload first segment
            var segment1 = new MemoryStream(segmentData1);
            await UploadToRAMAsync(segment1, (uint)segment1.Length, segmentOffset1, blockSize, token);

            // Upload second segment
            var segment2 = new MemoryStream(segmentData2);
            await UploadToRAMAsync(segment2, (uint)segment2.Length, segmentOffset2, blockSize, token);

            // Start the softloader 0x400be5ac
            await _loader.MemEndAsync(0, entryPoint, token);

            // Wait for OHAI signal
            await _loader.WaitForOHAIAsync(token);
        }

        protected abstract Task UploadAsync(Stream stream, uint size, uint offset, CancellationToken token);
    }




    private class FirmwareFlashSender : FirmwareSender
    {
        private readonly IFlashLoader loader;

        public FirmwareFlashSender(IFlashLoader loader)
        {
            this.loader = loader;
        }
    }
    private class FirmwareFlashDeflatedSender : FirmwareSender
    {
        private readonly IFlashDeflLoader loader;

        public FirmwareFlashDeflatedSender(IFlashDeflLoader loader)
        {
            this.loader = loader;
        }
    }

    private class FirmwareRAMSender : FirmwareSender
    {
        private readonly IMemLoader loader;

        public FirmwareRAMSender(IMemLoader loader)
        {
            this.loader = loader;
        }
    }








    /// <summary>
    /// Flashes
    /// </summary>
    public class RAMFlasher
    {
        private readonly Loader _loader;

        public RAMFlasher(Loader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// Flashes the softloader to RAM.
        /// </summary>
        public async Task FlashSoftLoaderAsync(uint blockSize, CancellationToken token = default)
        {
            // Upload first segment
            var segment1 = new MemoryStream(segmentData1);
            await UploadToRAMAsync(segment1, (uint)segment1.Length, segmentOffset1, blockSize, token);

            // Upload second segment
            var segment2 = new MemoryStream(segmentData2);
            await UploadToRAMAsync(segment2, (uint)segment2.Length, segmentOffset2, blockSize, token);

            // Start the softloader 0x400be5ac
            await _loader.MemEndAsync(0, entryPoint, token);

            // Wait for OHAI signal
            await _loader.WaitForOHAIAsync(token);
        }

        /// <summary>
        /// Uploads data to RAM in blocks.
        /// </summary>
        private async Task UploadToRAMAsync(Stream stream, uint size, uint offset, uint blockSize, CancellationToken token)
        {
            uint blocks = (size + blockSize - 1) / blockSize;

            // Begin memory transfer
            await _loader.MemBeginAsync(size, blocks, blockSize, offset, token);

            // Send data in blocks
            for (uint i = 0; i < blocks; i++)
            {
                uint srcInd = i * blockSize;
                uint len = size - srcInd;
                if (len > blockSize)
                    len = blockSize;

                byte[] buffer = new byte[len];
                int bytesRead = await stream.ReadAsync(buffer, 0, (int)len, token);
                if (bytesRead != len)
                    break;

                await _loader.MemDataAsync(buffer, i, token);
            }
        }
    }
}

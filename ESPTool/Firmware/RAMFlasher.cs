using ESPTool.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESPTool.Firmware
{
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

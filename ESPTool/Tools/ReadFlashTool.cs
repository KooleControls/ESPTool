using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace EspDotNet.Tools
{
    public class ReadFlashTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();
        public uint SectorSize { get; set; } = 4096; // Flash sector size
        public uint BlockSize { get; set; } = 64; // Read block size
        
        private readonly SoftLoader _softLoader;
        private readonly Communicator _communicator;

        public ReadFlashTool(SoftLoader softLoader, Communicator communicator)
        {
            _softLoader = softLoader;
            _communicator = communicator;
        }

        /// <summary>
        /// Reads data from flash memory.
        /// </summary>
        /// <param name="address">The flash address to start reading from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <returns>The data read from flash.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the read operation fails.</exception>
        public async Task<byte[]> ReadFlashAsync(uint address, uint size, CancellationToken token)
        {
            // Begin flash read
            await _softLoader.FlashReadBeginAsync(address, size, SectorSize, BlockSize, token);

            // Read data in chunks
            var data = new List<byte>();
            uint totalReceived = 0;

            while (totalReceived < size)
            {
                // Read frame containing flash data
                var frame = await _communicator.ReadFrameAsync(token);
                if (frame?.Data == null)
                    throw new InvalidOperationException("Failed to receive flash data frame.");

                data.AddRange(frame.Data);
                totalReceived += (uint)frame.Data.Length;

                // Send acknowledgment with bytes received
                await _softLoader.FlashReadAckAsync(totalReceived, token);

                // Update progress
                Progress.Report((float)totalReceived / size);

                // Validate data length for intermediate frames
                if (totalReceived < size && frame.Data.Length < SectorSize)
                {
                    throw new InvalidOperationException(
                        $"Corrupt data, expected {SectorSize:X} bytes but received {frame.Data.Length:X} bytes.");
                }
            }

            if (totalReceived > size)
                throw new InvalidOperationException("Read more data than expected.");

            // Read MD5 digest frame for verification
            var digestFrame = await _communicator.ReadFrameAsync(token);
            if (digestFrame?.Data == null || digestFrame.Data.Length != 16)
                throw new InvalidOperationException($"Expected 16-byte digest, got {digestFrame?.Data?.Length ?? 0} bytes.");

            // Verify MD5 checksum
            var flashData = data.ToArray();
            using (var md5 = MD5.Create())
            {
                var computedHash = md5.ComputeHash(flashData);
                var expectedHash = digestFrame.Data;

                for (int i = 0; i < 16; i++)
                {
                    if (computedHash[i] != expectedHash[i])
                    {
                        var expected = BitConverter.ToString(expectedHash).Replace("-", "");
                        var computed = BitConverter.ToString(computedHash).Replace("-", "");
                        throw new InvalidOperationException(
                            $"Digest mismatch: expected {expected}, got {computed}");
                    }
                }
            }

            return flashData;
        }

        /// <summary>
        /// Reads data from flash memory and saves it to a file.
        /// </summary>
        /// <param name="address">The flash address to start reading from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <param name="outputPath">The path where to save the read data.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        public async Task ReadFlashToFileAsync(uint address, uint size, string outputPath, CancellationToken token)
        {
            var data = await ReadFlashAsync(address, size, token);
            await File.WriteAllBytesAsync(outputPath, data, token);
        }
    }
}
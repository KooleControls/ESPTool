using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using System;
using System.IO;
using System.Linq;
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
        /// A wrapper stream that computes MD5 hash while writing to the underlying stream.
        /// </summary>
        private class MD5Stream : Stream
        {
            private readonly Stream _baseStream;
            private readonly MD5 _md5;

            public MD5Stream(Stream baseStream)
            {
                _baseStream = baseStream;
                _md5 = MD5.Create();
            }

            public byte[] GetHash() => _md5.Hash ?? throw new InvalidOperationException("Hash not computed yet.");

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => _baseStream.CanWrite;
            public override long Length => _baseStream.Length;
            public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

            public override void Flush() => _baseStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

            public override void SetLength(long value) => _baseStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count)
            {
                _md5.TransformBlock(buffer, offset, count, null, 0);
                _baseStream.Write(buffer, offset, count);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _md5.TransformBlock(buffer, offset, count, null, 0);
                await _baseStream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    _md5.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Reads data from flash memory and writes it to the provided stream.
        /// </summary>
        /// <param name="address">The flash address to start reading from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <param name="outputStream">The stream to write the read data to.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="InvalidOperationException">Thrown if the read operation fails.</exception>
        public async Task ReadFlashAsync(uint address, uint size, Stream outputStream, CancellationToken token)
        {
            await ReadFlashAsync(address, size, outputStream, false, token);
        }

        /// <summary>
        /// Reads data from flash memory and writes it to the provided stream with optional MD5 verification.
        /// </summary>
        /// <param name="address">The flash address to start reading from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <param name="outputStream">The stream to write the read data to.</param>
        /// <param name="verifyMd5">Whether to verify the MD5 checksum after reading.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="InvalidOperationException">Thrown if the read operation fails or MD5 verification fails.</exception>
        public async Task ReadFlashAsync(uint address, uint size, Stream outputStream, bool verifyMd5, CancellationToken token)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new ArgumentException("Output stream must be writable.", nameof(outputStream));

            // Use MD5 stream for verification if requested
            Stream targetStream = verifyMd5 ? new MD5Stream(outputStream) : outputStream;
            MD5Stream? md5Stream = targetStream as MD5Stream;

            try
            {
                // Begin flash read
                await _softLoader.FlashReadBeginAsync(address, size, SectorSize, BlockSize, token);

                uint totalReceived = 0;

                while (totalReceived < size)
                {
                    // Read frame containing flash data
                    var frame = await _communicator.ReadFrameAsync(token);
                    if (frame?.Data == null)
                        throw new InvalidOperationException("Failed to receive flash data frame.");

                    // Write data to stream
                    await targetStream.WriteAsync(frame.Data, 0, frame.Data.Length, token);
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

                // Perform MD5 verification if requested
                if (verifyMd5 && md5Stream != null)
                {
                    // Finalize MD5 computation
                    md5Stream.Dispose();
                    var computedHash = md5Stream.GetHash();

                    // Get expected hash from device
                    var expectedHash = await _softLoader.SPI_FLASH_MD5(address, size, token);

                    // Compare hashes
                    for (int i = 0; i < 16; i++)
                    {
                        if (computedHash[i] != expectedHash[i])
                        {
                            var expected = BitConverter.ToString(expectedHash).Replace("-", "");
                            var computed = BitConverter.ToString(computedHash).Replace("-", "");
                            throw new InvalidOperationException(
                                $"MD5 verification failed: expected {expected}, got {computed}");
                        }
                    }
                }
            }
            finally
            {
                if (md5Stream != null && md5Stream != outputStream)
                {
                    md5Stream.Dispose();
                }
            }
        }
    }
}
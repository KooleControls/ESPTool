using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using System;
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
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new ArgumentException("Output stream must be writable.", nameof(outputStream));

            // Begin flash read
            await _softLoader.FlashReadBeginAsync(address, size, SectorSize, BlockSize, token);

            // Wrap the output stream with MD5 computation
            using var md5Stream = new MD5Stream(outputStream);
            uint totalReceived = 0;

            while (totalReceived < size)
            {
                // Read frame containing flash data
                var frame = await _communicator.ReadFrameAsync(token);
                if (frame?.Data == null)
                    throw new InvalidOperationException("Failed to receive flash data frame.");

                // Write data to stream (and compute MD5)
                await md5Stream.WriteAsync(frame.Data, 0, frame.Data.Length, token);
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

            // Finalize MD5 computation
            md5Stream.Dispose(); // This finalizes the MD5 hash

            // Read MD5 digest frame for verification
            var digestFrame = await _communicator.ReadFrameAsync(token);
            if (digestFrame?.Data == null || digestFrame.Data.Length != 16)
                throw new InvalidOperationException($"Expected 16-byte digest, got {digestFrame?.Data?.Length ?? 0} bytes.");

            // Verify MD5 checksum
            var computedHash = md5Stream.GetHash();
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
    }
}
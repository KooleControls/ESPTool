using EspDotNet.Communication;
using EspDotNet.Loaders.SoftLoader;
using System.Security.Cryptography;

namespace EspDotNet.Tools
{
    public class ReadFlashTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();
        public uint SectorSize { get; set; } = 4096;
        public uint BlockSize { get; set; } = 4096;
        public uint MaxInFlight { get; set; } = 1;

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

            public byte[] FinalizeAndGetHash()
            {
                _md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return _md5.Hash ?? throw new InvalidOperationException("Hash not computed yet.");
            }

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
                _md5.Dispose();
                base.Dispose(disposing);
            }
        }


        public async Task ReadFlashAsync(uint address, uint size, Stream outputStream, CancellationToken token)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new ArgumentException("Output stream must be writable.", nameof(outputStream));

            using var md5Stream = new MD5Stream(outputStream);

            await _softLoader.FlashReadBeginAsync(address, size, BlockSize, MaxInFlight, token);

            uint totalReceived = 0;

            while (totalReceived < size)
            {
                totalReceived += await ReadFlashBlockAsync(md5Stream, totalReceived, token);
                Progress.Report((float)totalReceived / size);
            }

            await VerifyMd5Async(address, size, md5Stream, token);
        }

        private async Task<uint> ReadFlashBlockAsync(MD5Stream md5Stream, uint totalReceived, CancellationToken token)
        {
            var frame = await _communicator.ReadFrameAsync(token);
            if (frame?.Data == null)
                throw new InvalidOperationException("Failed to receive flash data frame.");

            await md5Stream.WriteAsync(frame.Data, 0, frame.Data.Length, token);

            await _softLoader.FlashReadAckAsync(totalReceived + (uint)frame.Data.Length, token);

            return (uint)frame.Data.Length;
        }







        private async Task VerifyMd5Async(uint address, uint size, MD5Stream stream, CancellationToken token)
        {
            var hashFrame = await _communicator.ReadFrameAsync(token);
            var computedHash = stream.FinalizeAndGetHash();
            var expectedHash = hashFrame?.Data ?? throw new Exception("Expected hash frame");

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
}
using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Utils;
using System.Net;
using System.Security.Cryptography;

namespace EspDotNet.Tools
{
    public class FlashDownloadTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();
        public uint BlockSize { get; set; } = 4096;
        public uint MaxInFlight { get; set; } = 1;

        private readonly SoftLoader _softLoader;
        private readonly Communicator _communicator;

        public FlashDownloadTool(SoftLoader softLoader, Communicator communicator)
        {
            _softLoader = softLoader;
            _communicator = communicator;
        }

        public Stream OpenFlashReadStream(uint address, uint size)
        {
            return new FlashReadStream(this, address, size);
        }

        public async Task ReadFlashAsync(uint address, uint size, Stream outputStream, CancellationToken token)
        {
            var flashStream = OpenFlashReadStream(address, size);
            await flashStream.CopyToAsync(outputStream, token);
        }

        public class FlashReadStream : Stream
        {
            private readonly FlashDownloadTool _tool;
            private readonly uint _totalSize;
            private uint _position = 0;

            public FlashReadStream(FlashDownloadTool tool, uint address, uint size)
            {
                _tool = tool;
                _position = address;
                _totalSize = size + address;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                uint size = (uint)count;

                if (_position >= _totalSize) return 0;
                if (_position + count >= _totalSize)
                    size = _totalSize - _position;

                await _tool._softLoader.FlashReadBeginAsync(_position, size, _tool.BlockSize, _tool.MaxInFlight, cancellationToken);
                MD5 md5 = MD5.Create();

                uint bytesRead = 0;
                while(bytesRead < size)
                {
                    var frame = await _tool._communicator.ReadFrameAsync(cancellationToken);
                    if (frame?.Data == null)
                        throw new IOException("Failed to receive flash data frame.");

                    Array.Copy(frame.Data, 0, buffer, offset + (int)bytesRead, frame.Data.Length);
                    md5.TransformBlock(frame.Data, 0, frame.Data.Length, null, 0);
                    bytesRead += (uint)frame.Data.Length;

                    await _tool._softLoader.FlashReadAckAsync(bytesRead, cancellationToken);
                    _position += (uint)frame.Data.Length;
                    _tool.Progress.Report((float)_position / _totalSize);
                }

                await VerifyMd5Async(md5, cancellationToken);

                return (int)size;
            }


            private async Task VerifyMd5Async(MD5 md5, CancellationToken token)
            {
                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                var hashFrame = await _tool._communicator.ReadFrameAsync(token);
                var computedHash = md5.Hash ?? throw new InvalidOperationException("Hash not computed yet.");
                var expectedHash = hashFrame?.Data ?? throw new Exception("Expected hash frame");

                for (int i = 0; i < 16; i++)
                {
                    if (computedHash[i] != expectedHash[i])
                    {
                        var expected = BitConverter.ToString(expectedHash).Replace("-", "");
                        var computed = BitConverter.ToString(computedHash).Replace("-", "");
                        throw new InvalidOperationException($"MD5 verification failed: expected \n    {expected}, got \n    {computed}");
                    }
                }
            }

            // Stream boilerplate
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _totalSize;
            public override long Position { get => _position; set => throw new NotSupportedException(); }
            public override void Flush() => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

    }
}


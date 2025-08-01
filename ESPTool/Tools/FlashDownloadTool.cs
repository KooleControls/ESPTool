using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Utils;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

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
            private uint _position; // Flash offset
            private readonly Queue<byte> _buffer = new();
            private readonly MD5 _md5 = MD5.Create();

            public FlashReadStream(FlashDownloadTool tool, uint address, uint size)
            {
                _tool = tool;
                _position = address;
                _totalSize = size + address;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (_position >= _totalSize && _buffer.Count == 0)
                    return 0;

                int bytesReturned = 0;

                bytesReturned += ReadFromBuffer(buffer, offset, count);
                bytesReturned += await ReadFromDeviceAsync(buffer, offset + bytesReturned, count - bytesReturned, cancellationToken);

                return bytesReturned;
            }

            private int ReadFromBuffer(byte[] buffer, int offset, int count)
            {
                int bytesRead = 0;
                while (_buffer.Count > 0 && bytesRead < count)
                {
                    buffer[offset + bytesRead++] = _buffer.Dequeue();
                }
                return bytesRead;
            }

            private async Task<int> ReadFromDeviceAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                int remainingBytes = (int)(_totalSize - _position);
                if (remainingBytes <= 0)
                    return 0;

                int toRead = Math.Max(count, (int)_tool.BlockSize);
                toRead = Math.Min(toRead, remainingBytes);

                _md5.Initialize();
                await _tool._softLoader.FlashReadBeginAsync(_position, (uint)toRead, _tool.BlockSize, _tool.MaxInFlight, cancellationToken);

                int bytesCopiedToCaller = 0;

                while (bytesCopiedToCaller < count && toRead > 0)
                {
                    var frame = await _tool._communicator.ReadFrameAsync(cancellationToken);
                    if (frame?.Data == null)
                        throw new IOException("Failed to receive flash data frame.");

                    _md5.TransformBlock(frame.Data, 0, frame.Data.Length, null, 0);

                    int remainingCallerBuffer = count - bytesCopiedToCaller;
                    int copyCount = Math.Min(remainingCallerBuffer, frame.Data.Length);

                    Array.Copy(frame.Data, 0, buffer, offset + bytesCopiedToCaller, copyCount);
                    bytesCopiedToCaller += copyCount;

                    // Buffer the rest if any
                    for (int i = copyCount; i < frame.Data.Length; i++)
                        _buffer.Enqueue(frame.Data[i]);

                    _position += (uint)frame.Data.Length;
                    toRead -= frame.Data.Length;

                    await _tool._softLoader.FlashReadAckAsync(_position, cancellationToken);
                    _tool.Progress.Report((float)_position / _totalSize);
                }

                _md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                await VerifyMd5Async(_md5, cancellationToken);

                return bytesCopiedToCaller;
            }


            private async Task VerifyMd5Async(MD5 md5, CancellationToken token)
            {
                var hashFrame = await _tool._communicator.ReadFrameAsync(token);
                var computedHash = md5.Hash ?? throw new InvalidOperationException("Hash not computed yet.");
                var expectedHash = hashFrame?.Data ?? throw new Exception("Expected hash frame");

                for (int i = 0; i < 16; i++)
                {
                    if (computedHash[i] != expectedHash[i])
                    {
                        var expected = BitConverter.ToString(expectedHash).Replace("-", "");
                        var computed = BitConverter.ToString(computedHash).Replace("-", "");
                        throw new InvalidOperationException($"MD5 verification failed:\n  Expected: {expected}\n  Computed: {computed}");
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


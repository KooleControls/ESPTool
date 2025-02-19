using EspDotNet.Loaders;
using EspDotNet.Tools.Firmware;
using EspDotNet.Utils;

namespace EspDotNet.Tools
{
    public class FirmwareUploadTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();

        private readonly ILoader _loader;
        private readonly FirmwareUploadConfig _config;

        public FirmwareUploadTool(ILoader loader)
        {
            _loader = loader;
            _config = new FirmwareUploadConfig();
        }

        public FirmwareUploadTool(ILoader loader, FirmwareUploadConfig config)
        {
            _loader = loader;
            _config = config;
        }

        public async Task UploadFirmwareAsync(IFirmwareProvider firmwareProvider, CancellationToken token)
        {
            Progress.Report(0);
            var totalSize = firmwareProvider.Segments.Sum(s => s.Size);
            float uploadedBytes = 0;

            foreach (var segment in firmwareProvider.Segments)
            {
                float segmentWeight = (float)segment.Size / totalSize; // Fractional contribution of this segment

                Action<float> reportProgress = (p) =>
                {
                    float segmentProgress = p * segmentWeight;
                    uploadedBytes += segment.Size * p;
                    Progress.Report(uploadedBytes / totalSize); // Report overall fraction 0-1
                };

                await UploadSegment(segment, _config.BlockSize, token, new Progress<float>(reportProgress));
            }

            // End memory transfer
            uint execute = (uint)(_config.ExecuteAfterSending ? 1 : 0);
            await SendEndAsync(execute, firmwareProvider.EntryPoint, token);
            Progress.Report(1);
        }


        private async Task UploadSegment(IFirmwareSegmentProvider segmentProvider, uint blockSize, CancellationToken token, IProgress<float> progress)
        {
            Stream data = await segmentProvider.GetStreamAsync(token);

            // Compress data if needed
            if (_config.UploadMethod == FirmwareUploadOptions.FlashDeflated)
            {
                data = new MemoryStream();
                ZlibCompressionHelper.CompressToZlibStream(await segmentProvider.GetStreamAsync(token), data);
                data.Position = 0;
            }

            uint size = (uint)data.Length;
            uint blocks = (size + blockSize - 1) / blockSize;
            uint offset = segmentProvider.Offset;

            // Begin memory transfer
            await SendBeginAsync(size, blocks, offset, blockSize, token);

            // Send data in blocks
            for (uint i = 0; i < blocks; i++)
            {
                uint srcInd = i * blockSize;
                uint len = size - srcInd;
                if (len > blockSize)
                    len = blockSize;

                byte[] buffer = new byte[len];
                int bytesRead = await data.ReadAsync(buffer, 0, (int)len, token);
                if (bytesRead != len)
                    break;

                await SendBlockAsync(buffer, i, token);
                progress.Report((float)(i + 1) / blocks); // Report 0-1 per segment
            }
        }

        private async Task SendBeginAsync(uint size, uint blocks, uint offset, uint blockSize, CancellationToken token)
        {
            switch (_config.UploadMethod)
            {
                case FirmwareUploadOptions.Flash:
                    await _loader.FlashBeginAsync(size, blocks, blockSize, offset, token);
                    break;
                case FirmwareUploadOptions.FlashDeflated:
                    await _loader.FlashDeflBeginAsync(size, blocks, blockSize, offset, token);
                    break;
                case FirmwareUploadOptions.Ram:
                    await _loader.MemBeginAsync(size, blocks, blockSize, offset, token);
                    break;
                default: throw new Exception();
            }
        }


        private async Task SendBlockAsync(byte[] block, uint blockNo, CancellationToken token)
        {
            switch (_config.UploadMethod)
            {
                case FirmwareUploadOptions.Flash:
                    await _loader.FlashDataAsync(block, blockNo, token);
                    break;
                case FirmwareUploadOptions.FlashDeflated:
                    await _loader.FlashDeflDataAsync(block, blockNo, token);
                    break;
                case FirmwareUploadOptions.Ram:
                    await _loader.MemDataAsync(block, blockNo, token);
                    break;
                default: throw new Exception();
            }
        }

        private async Task SendEndAsync(uint execute, uint entryPoint, CancellationToken token)
        {
            switch (_config.UploadMethod)
            {
                case FirmwareUploadOptions.Flash:
                    await _loader.FlashEndAsync(execute, entryPoint, token);
                    break;
                case FirmwareUploadOptions.FlashDeflated:
                    await _loader.FlashDeflEndAsync(execute, entryPoint, token);
                    break;
                case FirmwareUploadOptions.Ram:
                    await _loader.MemEndAsync(execute, entryPoint, token);
                    break;
                default: throw new Exception();
            }
        }
    }




}

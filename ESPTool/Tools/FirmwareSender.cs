using ESPTool.Loaders;
using ESPTool.Models;

namespace ESPTool.Tools
{
    public class FirmwareSender
    {
        public int BlockSize { get; set; } = 1024;
        public FirmwareUploadOptions UploadMethod { get; set; }
        public bool ExecuteAfterSending { get; set; } = false;


        private readonly ILoader _loader;

        public FirmwareSender(ILoader loader)
        {
            _loader = loader;
        }

        public async Task UploadFirmwareAsync(Firmware firmware, CancellationToken token = default)
        {
            foreach (var segment in firmware.Segments)
            {
                await UploadSegment(segment, (uint)BlockSize, token);
            }

            // End memory transfer
            uint execute = (uint)(ExecuteAfterSending ? 1 : 0);
            await SendEndAsync(execute, firmware.EntryPoint, token);
        }


        private async Task UploadSegment(FirmwareSegment segment, uint blockSize, CancellationToken token)
        {
            MemoryStream data = new MemoryStream(segment.Data);
            uint size = (uint)data.Length;
            uint blocks = (size + blockSize - 1) / blockSize;
            uint offset = segment.Offset;

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
            }
        }

        private async Task SendBeginAsync(uint size, uint blocks, uint offset, uint blockSize, CancellationToken token)
        {
            switch (UploadMethod)
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
            switch (UploadMethod)
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
            switch (UploadMethod)
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

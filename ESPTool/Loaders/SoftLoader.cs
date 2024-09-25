using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class SoftLoader : Loader
    {
        public SoftLoader(Communicator communicator) : base(communicator, new SoftLoaderCommandExecutor(communicator))
        {
        }

        /// <summary>
        /// Changes the baud rate for communication in the SoftLoader.
        /// </summary>
        /// <param name="baud">The new baud rate to set.</param>
        /// <param name="oldBaud">The current baud rate.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the token.</exception>
        public override async Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x0F)
                .AppendPayload(BitConverter.GetBytes(baud))
                .AppendPayload(BitConverter.GetBytes(oldBaud))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to change baud rate.");
        }

        /// <summary>
        /// Calculates and verifies the MD5 checksum of a given flash region.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the token.</exception>
        public async Task<byte[]> SPI_FLASH_MD5(uint address, uint size, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x13)
                .AppendPayload(BitConverter.GetBytes(address))
                .AppendPayload(BitConverter.GetBytes(size))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(BitConverter.GetBytes(0))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to compute MD5 checksum.");

            return response.Payload.Take(16).ToArray(); // Return the MD5 checksum (first 16 bytes)
        }

        /// <summary>
        /// Begins the flash process using compressed data.
        /// </summary>
        public async Task FlashDeflBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x10)
                .AppendPayload(BitConverter.GetBytes(size))
                .AppendPayload(BitConverter.GetBytes(blocks))
                .AppendPayload(BitConverter.GetBytes(blockSize))
                .AppendPayload(BitConverter.GetBytes(offset))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to begin flash with compressed data.");
        }

        /// <summary>
        /// Sends compressed flash data.
        /// </summary>
        public async Task FlashDeflDataAsync(byte[] blockData, uint seq, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x11)
                .RequiresChecksum()
                .AppendPayload(BitConverter.GetBytes(blockData.Length))
                .AppendPayload(BitConverter.GetBytes(seq))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(blockData)
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to send compressed flash data.");
        }

        /// <summary>
        /// Ends the flash process using compressed data.
        /// </summary>
        public async Task FlashDeflEndAsync(uint executeFlags, uint entryPoint, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x12)
                .AppendPayload(BitConverter.GetBytes(executeFlags))
                .AppendPayload(BitConverter.GetBytes(entryPoint))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to end flash with compressed data.");
        }

        /// <summary>
        /// Erases the entire flash memory.
        /// </summary>
        public async Task EraseFlashAsync(CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0xD0)
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (!response.Success)
                throw new InvalidOperationException("Failed to erase flash memory.");
        }
    }




}

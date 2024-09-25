using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class Loader
    {
        private readonly byte[] OHAI = { 0x4F, 0x48, 0x41, 0x49 };
        protected readonly Communicator _communicator;
        protected readonly CommandExecutor _commandExecutor;

        public Loader(Communicator communicator)
        {
            _communicator = communicator;
            _commandExecutor = new CommandExecutor(communicator);
        }

        protected Loader(Communicator communicator, CommandExecutor executor)
        {
            _communicator = communicator;
            _commandExecutor = executor;
        }

        #region Supported by software loader and ROM loaders

        /// <summary>
        /// Begins the flash process.
        /// </summary>
        public virtual async Task FlashBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x02)
                .AppendPayload(BitConverter.GetBytes(size))
                .AppendPayload(BitConverter.GetBytes(blocks))
                .AppendPayload(BitConverter.GetBytes(blockSize))
                .AppendPayload(BitConverter.GetBytes(offset))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("FlashBegin failed");
        }

        /// <summary>
        /// Sends flash data.
        /// </summary>
        public virtual async Task FlashDataAsync(byte[] blockData, uint seq, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x03)
                .RequiresChecksum()
                .AppendPayload(BitConverter.GetBytes(blockData.Length))
                .AppendPayload(BitConverter.GetBytes(seq))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(blockData)
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("FlashData failed");
        }

        /// <summary>
        /// Ends the flash process.
        /// </summary>
        public virtual async Task FlashEndAsync(uint execute, uint entryPoint, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x04)
                .AppendPayload(BitConverter.GetBytes(execute))
                .AppendPayload(BitConverter.GetBytes(entryPoint))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("FlashEnd failed");
        }

        /// <summary>
        /// Begins memory upload.
        /// </summary>
        public virtual async Task MemBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x05)
                .AppendPayload(BitConverter.GetBytes(size))
                .AppendPayload(BitConverter.GetBytes(blocks))
                .AppendPayload(BitConverter.GetBytes(blockSize))
                .AppendPayload(BitConverter.GetBytes(offset))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("MemBegin failed");
        }

        /// <summary>
        /// Ends memory upload.
        /// </summary>
        public virtual async Task MemEndAsync(uint execute, uint entryPoint, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x06)
                .AppendPayload(BitConverter.GetBytes(execute))
                .AppendPayload(BitConverter.GetBytes(entryPoint))
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("MemEnd failed");
        }

        /// <summary>
        /// Sends memory data.
        /// </summary>
        public virtual async Task MemDataAsync(byte[] blockData, uint seq, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x07)
                .RequiresChecksum()
                .AppendPayload(BitConverter.GetBytes(blockData.Length))
                .AppendPayload(BitConverter.GetBytes(seq))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(BitConverter.GetBytes(0))
                .AppendPayload(blockData)
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("MemData failed");
        }

        /// <summary>
        /// Synchronizes with the loader.
        /// </summary>
        public virtual async Task SyncAsync(CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x08)
                .AppendPayload(new byte[] { 0x07, 0x07, 0x12, 0x20, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 })
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("Sync failed");
        }

        public virtual async Task<UInt32> ReadRegisterAsync(UInt32 address, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
               .WithCommand(0x0a)
               .AppendPayload(BitConverter.GetBytes(address))
               .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new Exception("Sync failed");

            return response.Value;
        }
        #endregion

        #region Supported by software loader and ESP32 ROM Loader

        /// <summary>
        /// Changes the baud rate for communication.
        /// </summary>
        public virtual async Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token)
        {
            throw new NotSupportedException("Changing baud rate is not supported by this loader.");
        }

        #endregion

        #region Misc

        /// <summary>
        /// Waits for the OHAI message.
        /// </summary>
        public async Task WaitForOHAIAsync(CancellationToken token)
        {
            Frame? frame = null;

            while (frame?.Data.SequenceEqual(OHAI) != true)
            {
                frame = await _communicator.ReadFrameAsync(token);
            }
        }

        #endregion

    }
}

using ESPTool.Commands;
using ESPTool.Communication;
using ESPTool.Loaders.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders.ESP32BootLoader
{
    public class ESP32BootLoader : ILoader, IBaudRateConfigurableLoader, IFlashLoader, IMemLoader
    {
        private readonly byte[] OHAI = { 0x4F, 0x48, 0x41, 0x49 };
        private readonly Communicator _communicator;
        private readonly BootLoaderCommandExecutor _commandExecutor;

        public ESP32BootLoader(Communicator communicator)
        {
            _communicator = communicator;
            _commandExecutor = new BootLoaderCommandExecutor(communicator);
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
            if (response.Success != true)
                throw new Exception($"FlashBegin failed {response.Error}");
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
            if (response.Success != true)
                throw new Exception($"FlashData failed {response.Error}");
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
            if (response.Success != true)
                throw new Exception($"FlashEnd failed {response.Error}");
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
            if (response.Success != true)
                throw new Exception($"MemBegin failed {response.Error}");
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
            if (response.Success != true)
                throw new Exception($"MemEnd failed {response.Error}");
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
            if (response.Success != true)
                throw new Exception($"MemData failed {response.Error}");
        }

        /// <summary>
        /// Synchronizes with the loader.
        /// </summary>
        public virtual async Task SyncAsync(CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x08)
                .AppendPayload(new byte[] { 0x07, 0x07, 0x12, 0x20, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 })
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response.Success != true)
                throw new Exception($"Synchronisation failed {response.Error}");
        }

        public virtual async Task<UInt32> ReadRegisterAsync(UInt32 address, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
               .WithCommand(0x0a)
               .AppendPayload(BitConverter.GetBytes(address))
               .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response.Success != true)
                throw new Exception($"Reading register failed {response.Error}");

            return response.Value;
        }
        #endregion

        #region Misc

        /// <summary>
        /// Waits for the OHAI message.
        /// </summary>
        public async Task WaitForOHAIAsync(CancellationToken token)
        {
            Frame? frame = await _communicator.ReadFrameAsync(token);

            while (frame?.Data.SequenceEqual(OHAI) != true)
            {
                frame = await _communicator.ReadFrameAsync(token);
            }
        }

        #endregion

        /// <summary>
        /// Changes the baud rate for communication in ESP32.
        /// </summary>
        /// <param name="baud">The new baud rate to set.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the token.</exception>
        public async Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x0F)
                .AppendPayload(BitConverter.GetBytes(baud))
                .AppendPayload(BitConverter.GetBytes(0))  // Adding second parameter with value 0
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new InvalidOperationException("Failed to change baud rate.");
        }
    }
}

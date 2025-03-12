using EspDotNet.Commands;
using EspDotNet.Communication;
using EspDotNet.Loaders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EspDotNet.Loaders.ESP32BootLoader
{
    public class ESP32BootLoader : ILoader
    {
        private readonly Communicator _communicator;
        private readonly BootLoaderCommandExecutor _commandExecutor;

        public ESP32BootLoader(Communicator communicator)
        {
            _communicator = communicator;
            _commandExecutor = new BootLoaderCommandExecutor(communicator);
        }


        /// <summary>
        /// Begins the flash process.
        /// </summary>
        public async Task FlashBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token)
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
        public async Task FlashDataAsync(byte[] blockData, uint seq, CancellationToken token)
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
        public async Task FlashEndAsync(uint execute, uint entryPoint, CancellationToken token)
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
        public async Task MemBeginAsync(uint size, uint blocks, uint blockSize, uint offset, CancellationToken token)
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
        public async Task MemEndAsync(uint execute, uint entryPoint, CancellationToken token)
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
        public async Task MemDataAsync(byte[] blockData, uint seq, CancellationToken token)
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
        public async Task<bool> SynchronizeAsync(CancellationToken token)
        {
            var request = new RequestCommandBuilder()
                .WithCommand(0x08)
                .AppendPayload(new byte[] { 0x07, 0x07, 0x12, 0x20, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 })
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            return response.Success;
        }

        public async Task<uint> ReadRegisterAsync(uint address, CancellationToken token)
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


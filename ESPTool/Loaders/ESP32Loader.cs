using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class ESP32Loader : Loader
    {
        public ESP32Loader(Communicator communicator) : base(communicator)
        {
        }

        /// <summary>
        /// Changes the baud rate for communication in ESP32.
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
                .AppendPayload(BitConverter.GetBytes(0))  // Adding second parameter with value 0
                .Build();

            var response = await _commandExecutor.ExecuteCommandAsync(request, token);
            if (response?.Success != true)
                throw new InvalidOperationException("Failed to change baud rate.");
        }
    }
}

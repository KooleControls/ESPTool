using EspDotNet.Commands;
using EspDotNet.Communication;

namespace EspDotNet.Loaders.ESP32BootLoader
{
    public class BootLoaderCommandExecutor
    {
        protected readonly Communicator _communicator;

        public BootLoaderCommandExecutor(Communicator communicator)
        {
            _communicator = communicator;
        }

        /// <summary>
        /// Sends a request frame and waits for a response frame.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the token.</exception>
        public async Task<ResponseCommand> ExecuteCommandAsync(RequestCommand requestCommand, CancellationToken token)
        {
            try
            {
                _communicator.ClearBuffer();

                // Convert the RequestCommand to a Frame
                Frame requestFrame = RequestToFrame(requestCommand);

                // Write the frame to the communicator
                await _communicator.WriteFrameAsync(requestFrame, token);

                // Read the response frame
                Frame responseFrame = await _communicator.ReadFrameAsync(token) ?? throw new Exception("No frame received");

                // Convert the response frame back to a ResponseCommand
                var response = FrameToResponse(responseFrame);

                // Check
                if (response.Command != requestCommand.Command)
                {
                    throw new Exception("Response didnt match");
                }
                return response;

            }

            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("Frame operation was canceled.", token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send or receive frame.", ex);
            }
        }

        private static Frame RequestToFrame(RequestCommand command)
        {
            List<byte> raw = new List<byte>
                {
                    command.Direction,
                    command.Command
                };
            raw.AddRange(BitConverter.GetBytes(command.Size));
            raw.AddRange(BitConverter.GetBytes(command.Checksum));
            raw.AddRange(command.Payload);
            return new Frame(raw.ToArray());
        }

        private static ResponseCommand FrameToResponse(Frame frame)
        {
            ResponseCommand response = new ResponseCommand();
            try
            {
                response.Direction = frame.Data[0];
                response.Command = frame.Data[1];
                response.Size = BitConverter.ToUInt16(frame.Data, 2);
                response.Value = BitConverter.ToUInt32(frame.Data, 4);
                response.Payload = frame.Data.Skip(8).ToArray();
                response.Success = response.Payload[response.Size - 4] == 0;
                BootLoaderResponseStatus status = (BootLoaderResponseStatus)response.Payload[response.Size - 3];
                response.Error = GeneralizeResponseStatus(status);
            }
            catch
            {
                response.Success = false;
            }
            return response;
        }


        private static ResponseCommandStatus GeneralizeResponseStatus(BootLoaderResponseStatus err)
        {
            return err switch
            {
                BootLoaderResponseStatus.Invalid => ResponseCommandStatus.Invalid,
                BootLoaderResponseStatus.Failed => ResponseCommandStatus.Failed,
                BootLoaderResponseStatus.InvalidCRC => ResponseCommandStatus.InvalidCRC,
                BootLoaderResponseStatus.WriteError => ResponseCommandStatus.WriteError,
                BootLoaderResponseStatus.ReadError => ResponseCommandStatus.ReadError,
                BootLoaderResponseStatus.ReadLenthError => ResponseCommandStatus.ReadLenthError,
                BootLoaderResponseStatus.DeflateError => ResponseCommandStatus.DeflateError,
                _ => ResponseCommandStatus.Unknown,
            };
        }
    }
}

using ESPTool.Communication;

namespace ESPTool.Commands
{
    public class CommandExecutor
    {
        protected readonly Communicator _communicator;

        public CommandExecutor(Communicator communicator)
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

        /// <summary>
        /// Converts a RequestCommand object into a Frame object.
        /// </summary>
        protected virtual Frame RequestToFrame(RequestCommand command)
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

        /// <summary>
        /// Converts a Frame object into a ResponseCommand object.
        /// </summary>
        protected virtual ResponseCommand FrameToResponse(Frame frame)
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
                response.Error = ((RomLoaderErrors)response.Payload[response.Size - 3]).ToResponseError();
            }
            catch
            {
                response.Success = false;
            }
            return response;
        }
    }
}

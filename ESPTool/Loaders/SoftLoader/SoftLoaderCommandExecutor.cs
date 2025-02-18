using ESPTool.Commands;
using ESPTool.Communication;

namespace ESPTool.Loaders.SoftLoader
{

    public class SoftLoaderCommandExecutor
    {
        protected readonly Communicator _communicator;

        public SoftLoaderCommandExecutor(Communicator communicator)
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

                //These 2 fields are switched around when compared to the documentation.
                response.Success = response.Payload[response.Size - 1] == 0;
                SoftLoaderResponseStatus status = (SoftLoaderResponseStatus)response.Payload[response.Size - 2];
                response.Error = GeneralizeResponseStatus(status);

                if (response.Error != ResponseCommandStatus.NoError)
                {
                    response.Success = false;
                }
            }
            catch
            {
                response.Success = false;
            }
            return response;
        }

        private static ResponseCommandStatus GeneralizeResponseStatus(SoftLoaderResponseStatus err)
        {
            return err switch
            {
                SoftLoaderResponseStatus.ESP_OK => ResponseCommandStatus.NoError,
                SoftLoaderResponseStatus.ESP_BAD_DATA_LEN => ResponseCommandStatus.Invalid,
                SoftLoaderResponseStatus.ESP_BAD_DATA_CHECKSUM => ResponseCommandStatus.InvalidCRC,
                SoftLoaderResponseStatus.ESP_BAD_BLOCKSIZE => ResponseCommandStatus.BadBlockSize,
                SoftLoaderResponseStatus.ESP_INVALID_COMMAND => ResponseCommandStatus.Failed,
                SoftLoaderResponseStatus.ESP_FAILED_SPI_OP => ResponseCommandStatus.FailedSPIOP,
                SoftLoaderResponseStatus.ESP_FAILED_SPI_UNLOCK => ResponseCommandStatus.FailedSPIUnlock,
                SoftLoaderResponseStatus.ESP_NOT_IN_FLASH_MODE => ResponseCommandStatus.NotInFlashMode,
                SoftLoaderResponseStatus.ESP_INFLATE_ERROR => ResponseCommandStatus.InflateError,
                SoftLoaderResponseStatus.ESP_NOT_ENOUGH_DATA => ResponseCommandStatus.NotEnoughData,
                SoftLoaderResponseStatus.ESP_TOO_MUCH_DATA => ResponseCommandStatus.TooMuchData,
                SoftLoaderResponseStatus.ESP_CMD_NOT_IMPLEMENTED => ResponseCommandStatus.Failed,
                _ => ResponseCommandStatus.Unknown,
            };
        }
    }
}

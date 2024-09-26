using ESPTool.Communication;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESPTool.Commands
{

    public class SoftLoaderCommandExecutor : CommandExecutor
    {
        public SoftLoaderCommandExecutor(Communicator communicator) : base(communicator)
        {
        }

        protected override ResponseCommand FrameToResponse(Frame frame)
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
                response.Error = ((SoftLoaderErrors)response.Payload[response.Size - 2]).ToResponseError();

                if (response.Error != ResponseErrors.NoError)
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
    }
}

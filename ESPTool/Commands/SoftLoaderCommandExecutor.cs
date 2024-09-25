using ESPTool.Com;

namespace ESPTool.CMD
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
                response.Payload = frame.Data.SubArray(8);

                //These 2 fields are switched around when compared to the documentation.
                response.Success = response.Payload[response.Size - 1] == 0;
                response.Error = ((SoftLoaderErrors)response.Payload[response.Size - 2]).ToGlobalError();
            }
            catch
            {
                response.Success = false;
            }
            return response;
        }
    }
}

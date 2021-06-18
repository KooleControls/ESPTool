using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class SoftLoader : Loader
    {

        public SoftLoader(Loader lod) : base(lod)
        {

        }


        public override async Task<ReplyCMD> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(baud),
                BitConverter.GetBytes(oldBaud)));
            return await DoFrame(request, ct);
        }


        protected override ReplyCMD ToCommand(Frame frame)
        {
            ReplyCMD cmd = new ReplyCMD();
            try
            {
                cmd.Direction = frame.Data[0];
                cmd.Command = frame.Data[1];
                cmd.Size = BitConverter.ToUInt16(frame.Data, 2);
                cmd.Value = BitConverter.ToUInt32(frame.Data, 4);
                cmd.Payload = frame.Data.SubArray(8);
                cmd.Success = cmd.Payload[cmd.Size - 2] == 0;
                cmd.Error = (Errors)cmd.Payload[cmd.Size - 1];
            }
            catch
            {
                cmd.Success = false;
            }
            return cmd;
        }

    }
}


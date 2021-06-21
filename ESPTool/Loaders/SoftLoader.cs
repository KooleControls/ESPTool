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



        public override async Task<ReplyCMD> FLASH_DEFL_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x10, false, Helpers.Concat(
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(blocks),
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return await DoFrame(request, ct);
        }

        public override async Task<ReplyCMD> FLASH_DEFL_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x11, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return await DoFrame(request, ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeFlags">0 = reboot, 1 = run user code</param>
        /// <param name="entryPoint"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task<ReplyCMD> FLASH_DEFL_END(UInt32 executeFlags, UInt32 entryPoint, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x12, false, Helpers.Concat(
                BitConverter.GetBytes(executeFlags),
                BitConverter.GetBytes(entryPoint)));
            return await DoFrame(request, ct);
        }

        public override async Task<ReplyCMD> ERASE_FLASH(CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0xd0, false, new byte[0]);
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


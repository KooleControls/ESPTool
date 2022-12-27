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

        public override async Task<Result> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(baud),
                BitConverter.GetBytes(oldBaud)));
            return ToResult(await DoFrame(request, ct));
        }

        //Note that the ESP32 ROM loader returns the md5sum as 32 hex encoded ASCII bytes, whereas the software loader returns the md5sum as 16 raw data bytes of MD5 followed by 2 status bytes.
        public override async Task<Result<byte[]>> SPI_FLASH_MD5(UInt32 address, UInt32 size, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x13, false, Helpers.Concat(
                BitConverter.GetBytes(address),
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0)));

            ReplyCMD reply = await DoFrame(request, ct);

            if (!reply.Success)
                return null;

            return ToResult(reply, reply.Payload.SubArray(0, 16));
        }

        public override async Task<Result> FLASH_DEFL_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x10, false, Helpers.Concat(
                BitConverter.GetBytes(size),        //stub expects number of bytes here, manages erasing internally. ROM expects rounded up to erase block size
                BitConverter.GetBytes(blocks),      
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return ToResult(await DoFrame(request, ct));
        }

        public override async Task<Result> FLASH_DEFL_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default)
        {
            //ROM code writes block to flash before ACKing
            //Stub ACKs when block is received, then writes to flash while receiving the block after it
            RequestCMD request = new RequestCMD(0x11, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return ToResult(await DoFrame(request, ct));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeFlags">0 = reboot, 1 = run user code</param>
        /// <param name="entryPoint"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public override async Task<Result> FLASH_DEFL_END(UInt32 executeFlags, UInt32 entryPoint, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x12, false, Helpers.Concat(
                BitConverter.GetBytes(executeFlags),
                BitConverter.GetBytes(entryPoint)));
            return ToResult(await DoFrame(request, ct));
        }

        public override async Task<Result> ERASE_FLASH(CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0xd0, false, new byte[0]);
            return ToResult(await DoFrame(request, ct));
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

                //These 2 fields are switched around when compared to the documentation.
                cmd.Success = cmd.Payload[cmd.Size - 1] == 0;
                cmd.Error = ((SoftLoaderErrors)cmd.Payload[cmd.Size - 2]).ToGlobalError();

                if (cmd.Error != Errors.NoError)
                {
                    cmd.Success = false;
                }               
            }
            catch
            {
                cmd.Success = false;
            }
            return cmd;
        }

    }
}


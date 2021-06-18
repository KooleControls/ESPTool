using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{

    public class Loader
    {
        readonly byte[] OHAI = new byte[] { 0x4F, 0x48, 0x41, 0x49 };
        public Communicator Com { get; set; } = new Communicator();

        public Loader()
        {
           
        }

        public Loader(Loader lod)
        {
            Com = lod.Com;
        }

        #region Supported by software loader and ROM loaders
        public virtual async Task<ReplyCMD> FLASH_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x02, false, Helpers.Concat(
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(blocks),
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return await DoFrame(request, ct);
        }

        public virtual async Task<ReplyCMD> FLASH_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x03, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return await DoFrame(request, ct);
        }

        public virtual async Task<ReplyCMD> FLASH_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x04, false, Helpers.Concat(
                BitConverter.GetBytes(execute),
                BitConverter.GetBytes(entryPoint)));
            return await DoFrame(request, ct);
        }
        public virtual async Task<ReplyCMD> MEM_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x05, false, Helpers.Concat(
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(blocks),
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return await DoFrame(request, ct);
        }

        public virtual async Task<ReplyCMD> MEM_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x06, false, Helpers.Concat(
                BitConverter.GetBytes(execute),
                BitConverter.GetBytes(entryPoint)));
            return await DoFrame(request, ct);
        }

        public virtual async Task<ReplyCMD> MEM_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x07, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return await DoFrame(request, ct);
        }

        public virtual async Task<ReplyCMD> SYNC(CancellationToken ct = default(CancellationToken))
        {
            RequestCMD tx = new RequestCMD(0x08, false, new byte[] { 0x07, 0x07, 0x12, 0x20, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 });
            return await DoFrame(tx, ct);
        }

        public virtual async Task<ReplyCMD> READ_REG(UInt32 address, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD tx = new RequestCMD(0x0a, false, BitConverter.GetBytes(address));
            return await DoFrame(tx, ct);
        }

        #endregion

        #region Supported by software loader and ESP32 ROM Loader


        public virtual async Task<ReplyCMD> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default(CancellationToken))
        {
            throw new Exception("This loader doens't support changing baudrate. Use ESP32loader or software loader");
        }
        public virtual async Task<ReplyCMD> FLASH_DEFL_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default(CancellationToken))
        {
            throw new Exception("Not supported, use another loader");
        }

        public virtual async Task<ReplyCMD> FLASH_DEFL_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default(CancellationToken))
        {
            throw new Exception("Not supported, use another loader");
        }

        public virtual async Task<ReplyCMD> FLASH_DEFL_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default(CancellationToken))
        {
            throw new Exception("Not supported, use another loader");
        }

        #endregion

        #region Supported by software loader only (ESP8266 & ESP32)

        #endregion

        #region Misc

        public async Task<bool> WaitForOHAI(CancellationToken ct = default(CancellationToken))
        {
            TaskCompletionSource<Frame> frameReplyPending = new TaskCompletionSource<Frame>();
            ct.Register(() => {
                frameReplyPending.TrySetCanceled();
            });

            Com.FrameRecieved += (sender, e) =>
            {
                if (e.Data.SequenceEqual(OHAI))
                    frameReplyPending?.TrySetResult(e);
            };

            Frame rx = await frameReplyPending.Task;
            return true;
        }

        public async Task<ReplyCMD> DoFrame(RequestCMD frame, CancellationToken ct = default(CancellationToken))
        {
            ReplyCMD rxFrame = new ReplyCMD() { Success = false, Error = Errors.UnknownError };
            try
            {
                TaskCompletionSource<Frame> frameReplyPending = new TaskCompletionSource<Frame>();

                ct.Register(() => {
                    frameReplyPending.TrySetCanceled();
                });

                Com.FrameRecieved += (sender, e) =>
                {
                    frameReplyPending?.TrySetResult(e);
                };

                Frame tx = FromCommand(frame);
                Com.SendFrame(tx);

                Frame rx = await frameReplyPending.Task;
                rxFrame = ToCommand(rx);
            }
            catch (TaskCanceledException)
            {

            }
            return rxFrame;
        }

        protected virtual ReplyCMD ToCommand(Frame frame)
        {
            ReplyCMD cmd = new ReplyCMD();
            try
            {
                cmd.Direction = frame.Data[0];
                cmd.Command = frame.Data[1];
                cmd.Size = BitConverter.ToUInt16(frame.Data, 2);
                cmd.Value = BitConverter.ToUInt32(frame.Data, 4);
                cmd.Payload = frame.Data.SubArray(8);
                cmd.Success = cmd.Payload[cmd.Size - 4] == 0;
                cmd.Error = (Errors)cmd.Payload[cmd.Size - 3];
            }
            catch
            {
                cmd.Success = false;
            }
            return cmd;
        }

        protected virtual Frame FromCommand(RequestCMD frame)
        {
            List<byte> raw = new List<byte>();
            raw.Add(frame.Direction);
            raw.Add(frame.Command);
            raw.AddRange(BitConverter.GetBytes(frame.Size));
            raw.AddRange(BitConverter.GetBytes(frame.Checksum));
            raw.AddRange(frame.Payload);
            return new Frame(raw.ToArray());
        }

        #endregion

    }
}

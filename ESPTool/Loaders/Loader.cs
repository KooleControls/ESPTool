using ESPTool.CMD;
using ESPTool.Com;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected virtual Result ToResult(ReplyCMD reply)
        {
            return new Result() { Success = reply.Success, Error = reply.Error };
        }

        protected virtual Result<T> ToResult<T>(ReplyCMD reply, T value)
        {
            return new Result<T> { Success = reply.Success, Error = reply.Error, Value = value};
        }

        #region Supported by software loader and ROM loaders
        public virtual async Task<Result> FLASH_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x02, false, Helpers.Concat(
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(blocks),
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result> FLASH_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x03, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result> FLASH_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x04, false, Helpers.Concat(
                BitConverter.GetBytes(execute),
                BitConverter.GetBytes(entryPoint)));
            return ToResult(await DoFrame(request, ct));
        }
        public virtual async Task<Result> MEM_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x05, false, Helpers.Concat(
                BitConverter.GetBytes(size),
                BitConverter.GetBytes(blocks),
                BitConverter.GetBytes(blockSize),
                BitConverter.GetBytes(offset))
                );
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result> MEM_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x06, false, Helpers.Concat(
                BitConverter.GetBytes(execute),
                BitConverter.GetBytes(entryPoint)));
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result> MEM_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x07, true, Helpers.Concat(
                BitConverter.GetBytes(blockData.Length),
                BitConverter.GetBytes(seq),
                BitConverter.GetBytes(0),
                BitConverter.GetBytes(0),
                blockData));
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result> SYNC(CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x08, false, new byte[] { 0x07, 0x07, 0x12, 0x20, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 });
            return ToResult(await DoFrame(request, ct));
        }

        public virtual async Task<Result<UInt32>> READ_REG(UInt32 address, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x0a, false, BitConverter.GetBytes(address));
            ReplyCMD reply = await DoFrame(request, ct);
            return ToResult(reply, reply.Value);
        }

        #endregion

        #region Supported by software loader and ESP32 ROM Loader


        public virtual Task<Result> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }
        public virtual Task<Result> FLASH_DEFL_BEGIN(UInt32 size, UInt32 blocks, UInt32 blockSize, UInt32 offset, CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);

        }

        public virtual Task<Result> FLASH_DEFL_DATA(byte[] blockData, UInt32 seq, CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result> FLASH_DEFL_END(UInt32 execute, UInt32 entryPoint, CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result<byte[]>> SPI_FLASH_MD5(UInt32 address, UInt32 size, CancellationToken ct = default)
        {
            return Task.FromResult(new Result<byte[]> { Success = false, Error = Errors.UnsupportedByLoader });
        }
        #endregion

        #region Supported by software loader only (ESP8266 & ESP32)
        public virtual Task<Result> ERASE_FLASH(CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        #endregion

        #region Misc

        public async Task<Result> WaitForOHAI(CancellationToken ct = default)
        {
            TaskCompletionSource<Result> frameReplyPending = new TaskCompletionSource<Result>();
            ct.Register(() => {
                frameReplyPending.TrySetResult(new Result { Error = Errors.TaskCancelled, Success = false });
            });

            Com.FrameRecieved += (sender, e) =>
            {
                if (e.Data.SequenceEqual(OHAI))
                    frameReplyPending?.TrySetResult(new Result { Error = Errors.NoError, Success = true });
            };

            return await frameReplyPending.Task;
        }

        public async Task<ReplyCMD> DoFrame(RequestCMD frame, CancellationToken ct = default)
        {
            ReplyCMD rxFrame = new ReplyCMD() { Success = false, Error = Errors.Unknown };
            try
            {
                TaskCompletionSource<Frame> frameReplyPending = new TaskCompletionSource<Frame>();

                ct.Register(() => {
                    frameReplyPending.TrySetCanceled();
                });

                Com.FrameRecieved += ((sender, e) =>
                {
                    frameReplyPending?.TrySetResult(e);
                });

                Frame tx = FromCommand(frame);
                Com.ClearBuffer();
                Com.SendFrame(tx);

                Frame rx = await frameReplyPending.Task;
                rxFrame = ToCommand(rx);
            }
            catch (TaskCanceledException)
            {
                rxFrame.Error = Errors.TaskCancelled;
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
                cmd.Error = ((RomLoaderErrors)cmd.Payload[cmd.Size - 3]).ToGlobalError();
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

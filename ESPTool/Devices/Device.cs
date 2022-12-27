using ESPTool.CMD;
using ESPTool.Firmware;
using ESPTool.Loaders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Devices
{

    public class Device
    {
        protected Loader Loader { get; set; } = new Loader();

        public Device()
        {

        }

        public Device(Device dev)
        {
            Loader = dev.Loader;
        }

        public Task<Result> OpenSerial(string name, int baud)
        {
            Loader.Com.OpenSerial(name, baud);
            return Task.FromResult(Result.OK);
        }

        public Task<Result> CloseSerial()
        {
            Loader.Com.CloseSerial();
            return Task.FromResult(Result.OK);
        }

        public async Task<Result> EnterBootloader(CancellationToken ct = default)
        {
            return await Loader.Com.EnterBootloader(ct);
        }

        public async Task<Result> Reset(CancellationToken ct = default)
        {
            return await Loader.Com.Reset(ct);
        }

        public async Task<Result> Sync(CancellationToken ct = default)
        {
            Result res = Result.UnknownError;
            bool done = false;

            int try_no = 0;
            while (!done && try_no < 100)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                ct.Register(() => cts.Cancel());
                cts.CancelAfter(100);

                res = await Loader.SYNC(cts.Token);
                if (res.Success)
                {
                    System.Threading.Thread.Sleep(100); //Reading a register right after syncing will sometimes fail. This delay will fix this, altough it isn't a very nice solution.
                    res = Result.OK;
                    done = true;
                }
                else
                {
                    if (ct.IsCancellationRequested)
                    {
                        res = Result.TaskCanceled;
                        done = true;
                    }
                    try_no++;
                }
            }
            return res;
        }

        public async Task<Result<UInt32>> ReadRegister(uint address, CancellationToken ct = default)
        {
            return await Loader.READ_REG(address, ct);
        }


        public async Task<Result<ChipTypes>> DetectChipType()
        {
            UInt32 CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; // This ROM address has a different value on each chip model
            Result<UInt32> reg = await ReadRegister(CHIP_DETECT_MAGIC_REG_ADDR);
            return new Result<ChipTypes> { Success = reg.Success, Error = reg.Error, Value = (ChipTypes)reg.Value };
        }

        public async Task<Result> ChangeBaud(int baud, CancellationToken ct = default)
        {
            int oldBaud = Loader.Com.GetBaud();
            Result result = await Loader.ChangeBaud(baud, oldBaud, ct);
            if(result.Success)
            {
                Loader.Com.ChangeBaud(baud);
            }

            return result;
        }

        public virtual Task<Result> UploadToRAM(FirmwareImage firmware, bool execute, CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result> UploadToFLASH(FirmwareImage firmware, bool execute, CancellationToken ct = default, IProgress<float> progress = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result> UploadToFLASHDeflated(FirmwareImage firmware, bool execute, CancellationToken ct = default, IProgress<float> progress = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result> StartStubloader(CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }

        public virtual Task<Result> EraseFlash(CancellationToken ct = default)
        {
            return Task.FromResult(Result.UnsupportedByLoader);
        }


    }
}

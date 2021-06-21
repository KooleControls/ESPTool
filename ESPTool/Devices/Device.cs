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
        public event EventHandler<float> OnProgressChanged;
        protected Loader Loader { get; set; } = new Loader();

        public Device()
        {

        }

        protected void ReportProgressChange(float val)
        {
            OnProgressChanged?.Invoke(this, val);
        }

        public Device(Device dev)
        {
            Loader = dev.Loader;
            this.OnProgressChanged = dev.OnProgressChanged;
        }

        public void OpenSerial(string name, int baud)
        {
            Loader.Com.OpenSerial(name, baud);
        }

        public void CloseSerial()
        {
            Loader.Com.CloseSerial();
        }

        public async Task<bool> EnterBootloader(CancellationToken ct = default)
        {
            return await Loader.Com.EnterBootloader(ct);
        }

        public async Task<bool> Reset(CancellationToken ct = default)
        {
            return await Loader.Com.Reset(ct);
        }

        public async Task<bool> Sync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                ct.Register(() => cts.Cancel());
                cts.CancelAfter(100);

                ReplyCMD frame = await Loader.SYNC(cts.Token);
                if (frame.Success)
                {
                    //Reading a register right after syncing will sometimes fail. This delay will fix this, altough it isn't a very nice solution.
                    ct.WaitHandle.WaitOne(100); 
                    return true;
                }
            }
            return false;
        }

        public async Task<uint> ReadRegister(uint address, CancellationToken ct = default)
        {
            ReplyCMD r = await Loader.READ_REG(address, ct);
            if (r.Success)
                return r.Value;
            else
                throw new Exception("Couln't read register");
        }


        public async Task<ChipTypes> DetectChipType()
        {
            UInt32 CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; // This ROM address has a different value on each chip model
            UInt32 reg = await ReadRegister(CHIP_DETECT_MAGIC_REG_ADDR);
            return (ChipTypes)reg;
        }

        public async Task<bool> ChangeBaud(int baud, CancellationToken ct = default)
        {
            int oldBaud = Loader.Com.GetBaud();
            ReplyCMD reply = await Loader.ChangeBaud(baud, oldBaud, ct);
            if(reply.Success)
            {
                Loader.Com.ChangeBaud(baud);
            }

            return reply.Success;
        }

        public Exception GetError([CallerMemberName] string callerName = "")
        {
            string device = this.GetType().Name;
            if (device == nameof(Device))
                device = "unknown";

            return new Exception($"Current device '{device}' doens't support the '{callerName}' function. Use '{nameof(DetectChipType)}' to detect the right device.");
        }

        public virtual async Task<bool> UploadToRAM(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken))
        {
            throw GetError();
        }

        public virtual async Task<bool> UploadToFLASH(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken))
        {
            throw GetError();
        }

        public virtual async Task<bool> UploadToFLASHDeflated(FirmwareImage firmware, bool execute, Action<double> progressCallback, CancellationToken ct = default(CancellationToken))
        {
            throw GetError();
        }

        public virtual async Task<bool> StartStubloader(CancellationToken ct = default(CancellationToken))
        {
            throw GetError();
        }

        public virtual async Task<bool> EraseFlash(CancellationToken ct = default(CancellationToken))
        {
            throw GetError();
        }


    }
}

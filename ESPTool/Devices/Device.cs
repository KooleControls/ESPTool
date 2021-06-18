using ESPTool.CMD;
using ESPTool.Loaders;
using System;
using System.Collections.Generic;
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

        public void OpenSerial(string name, int baud)
        {
            Loader.Com.OpenSerial(name, baud);
        }

        public async Task<bool> EnterBootloader(CancellationToken ct = default)
        {
            return await Loader.Com.EnterBootloader(ct);
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
                    return true;
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
    }
}

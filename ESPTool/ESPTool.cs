using ESPTool.Devices;
using ESPTool.Firmware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool
{

    

    public class ESPTool
    {
        public event EventHandler<float> OnProgressValueChanged;
        public event EventHandler<string> OnProgressMessage;
        Device device = new Device();
        public ESPTool()
        {
            //programmer.OpenSerial("COM30", 115200);
            device.OnProgressChanged += (sender, e) => OnProgressValueChanged?.Invoke(this, e);
        }



        private async Task<bool> Initialize(string com, int baudrate, CancellationToken ct = default)
        {

            bool suc = true;

            device.OpenSerial(com, 115200);
            OnProgressMessage?.Invoke(this, $"Serialport {com} opened");

            if (suc) suc &= await device.EnterBootloader(ct);
            OnProgressMessage?.Invoke(this, $"Bootloader started {(suc ? "sucsesfully" : "failed")}.");

            if (suc) suc &= await device.Sync(ct);
            OnProgressMessage?.Invoke(this, $"Device synced {(suc ? "sucsesfully" : "failed")}.");


            if (suc)
            {
                ChipTypes ty = await device.DetectChipType();
                OnProgressMessage?.Invoke(this, $"Chip {ty.ToString()} detected.");

                if (ty == ChipTypes.ESP32)
                {
                    device = new ESP32(device);
                }
                else
                {
                    OnProgressMessage?.Invoke(this, $"Wrong device detected.");
                    suc = false;
                }
            }

            if (suc) suc &= await device.StartStubloader();
            OnProgressMessage?.Invoke(this, $"Stubloader uploaded {(suc ? "sucsesfully" : "failed")}.");

            if (suc) suc &= await device.ChangeBaud(baudrate);
            OnProgressMessage?.Invoke(this, $"Baudrate changed to {baudrate} {(suc ? "sucsesfully" : "failed")}.");

            return suc;
        }

        private async Task<bool> Finalize() //right now not cancelable, really should do these things. 
        {
            bool suc = true;
            device.CloseSerial();
            OnProgressMessage?.Invoke(this, $"Com closed.");
            return suc;
        }



        public async Task<bool> FlashFirmware(string com, int baudrate, FirmwareImage fi, CancellationToken ct = default)
        {
            bool suc = await Initialize(com, baudrate, ct);
            if (suc) suc &= await device.UploadToFLASH(fi, false, ct);
            if (suc) suc &= await device.Reset(ct);
            await Finalize();
            return suc;
        }

        public async Task<bool> Erase(string com, int baudrate, CancellationToken ct = default)
        {
            bool suc = await Initialize(com, baudrate, ct);
            OnProgressMessage?.Invoke(this, $"Erasing flash");
            if (suc) suc &= await device.EraseFlash(ct);
            OnProgressMessage?.Invoke(this, $"Erasing flash {baudrate} {(suc ? "done" : "failed")}.");
            OnProgressMessage?.Invoke(this, $"Resetting device.");
            if (suc) suc &= await device.Reset(ct);
            await Finalize();
            return suc;
        }

    }
}

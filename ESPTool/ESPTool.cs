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

        Device device = new Device();
        public ESPTool()
        {
            //programmer.OpenSerial("COM30", 115200);
        }



        private async Task<bool> Initialize(string com, int baudrate, Action<ProgressReport> progressCallback, CancellationToken ct = default)
        {

            bool suc = true;

            device.OpenSerial(com, 115200);
            progressCallback(new ProgressReport(0.1f, "Serial port opened"));

            if (suc) suc &= await device.EnterBootloader(ct);
            progressCallback(new ProgressReport(0.2f, $"Bootloader started {(suc?"sucsesfully": "failed")}."));

            if (suc) suc &= await device.Sync(ct);
            progressCallback(new ProgressReport(0.3f, $"Device synced {(suc ? "sucsesfully" : "failed")}."));


            if (suc)
            {
                ChipTypes ty = await device.DetectChipType();
                progressCallback(new ProgressReport(0.4f, $"Chip {ty.ToString()} detected."));

                if (ty == ChipTypes.ESP32)
                {
                    device = new ESP32(device);
                }
                else
                {
                    progressCallback(new ProgressReport(0.4f, $"Wrong device detected."));
                    suc = false;
                }
            }

            if (device is ESP32 esp)
            {
                if (suc) suc &= await esp.StartStubloader();
                progressCallback(new ProgressReport(0.5f, $"Stubloader uploaded {(suc ? "sucsesfully" : "failed")}."));


                if (suc) suc &= await esp.ChangeBaud(baudrate);
                progressCallback(new ProgressReport(0.6f, $"Baudrate changed to {baudrate} {(suc ? "sucsesfully" : "failed")}."));
            }
            else
                suc = false;
            return suc;
        }




        public async Task<bool> FlashFirmware(string com, int baudrate, Action<ProgressReport> progressCallback, FirmwareImage fi, CancellationToken ct = default)
        {
            bool suc = await Initialize(com, baudrate, progressCallback, ct);

            if(suc)
            {
                if (device is ESP32 esp)
                {
                    suc &= await esp.UploadToFLASH(fi, false, progressCallback, ct);
                }
            }
            return suc;
        }
    }

    public class ProgressReport
    {
        public float Progress { get; set; }
        public string Message { get; set; }

        public ProgressReport(float progress, string message)
        {
            Progress = progress;
            Message = message;
        }

        public ProgressReport(float progress)
        {
            Progress = progress;
        }
    }
}

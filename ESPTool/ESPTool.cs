using ESPTool.Devices;
using ESPTool.Firmware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool
{

    public class ESPTool
    {
        Device device = new Device();
        public TextWriter Logger { get; set; } = Console.Out;


        public ESPTool()
        {
            //programmer.OpenSerial("COM30", 115200);
        }



        private async Task<Result> Initialize(string com, int baudrate, CancellationToken ct = default)
        {

            Result result = Result.OK;

            device.OpenSerial(com, 115200);
            Logger.WriteLine($"Serialport {com} opened");

            if (result.Success) 
                result = await device.EnterBootloader(ct);
            Logger.WriteLine( $"Bootloader started {(result.Success ? "sucsesfully" : "failed")}.");

            if (result.Success) 
                result = await device.Sync(ct);
            Logger.WriteLine( $"Device synced {(result.Success ? "sucsesfully" : "failed")}.");


            if (result.Success)
            {
                Result<ChipTypes> resChipType = await device.DetectChipType();
                result = resChipType;
                if (result.Success)
                {
                    ChipTypes ty = resChipType.Value;
                    Logger.WriteLine( $"Chip {ty.ToString()} detected.");
                    if (ty == ChipTypes.ESP32)
                    {
                        device = new ESP32(device);
                    }
                    else
                    {
                        Logger.WriteLine( $"Wrong device detected.");
                        result = Result.WrongChip;
                    }
                }
            }

            if (result.Success) 
                result = await device.StartStubloader();
            Logger.WriteLine( $"Stubloader uploaded {(result.Success ? "sucsesfully" : "failed")}.");

            if (result.Success) 
                result = await device.ChangeBaud(baudrate);
            Logger.WriteLine( $"Baudrate changed to {baudrate} {(result.Success ? "sucsesfully" : "failed")}.");

            return result;
        }

        private async Task<Result> Finalize() //right now not cancelable, really should do these things. 
        {
            Result res = await device.CloseSerial();
            if(res.Success)
                Logger.WriteLine( $"Com closed.");
            return res;
        }



        public async Task<Result> FlashFirmware(string com, int baudrate, FirmwareImage fi, bool deflated, CancellationToken ct = default, Progress<float> progress = default)
        {
            Result result = await Initialize(com, baudrate, ct);
            if(deflated)
            {
                if (result.Success) 
                    result = await device.UploadToFLASHDeflated(fi, false, ct);
            }
            else
            {
                if (result.Success) 
                    result = await device.UploadToFLASH(fi, false, ct);
            }
            if (result.Success) 
                result = await device.Reset(ct);

            Result resFin = await Finalize();

            if (result.Success)
                result = resFin;

            return result;
        }

        public async Task<Result> Erase(string com, int baudrate, CancellationToken ct = default)
        {
            Result result = await Initialize(com, baudrate, ct);
            Logger.WriteLine( $"Erasing flash");
            if (result.Success) result = await device.EraseFlash(ct);
            Logger.WriteLine( $"Erasing flash {(result.Success ? "done" : "failed")}.");
            Logger.WriteLine( $"Resetting device.");
            if (result.Success) result = await device.Reset(ct);
            Result resFin = await Finalize();

            if (result.Success)
                result = resFin;

            return result;
        }

    }
}

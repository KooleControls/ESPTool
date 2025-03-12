using EspDotNet.Loaders.SoftLoader;
using EspDotNet.Tools;
using EspDotNet.Tools.Firmware;
using System.Diagnostics;

namespace EspDotNet
{
    public class Example
    {
        ChipTypes chipType;
        SoftLoader? softloader;
        ESPTool espTool;
        int baudRate = 921600;

        public Example()
        {
            espTool = new ESPTool();
            chipType = ChipTypes.Unknown;
        }

        public async Task Init(CancellationToken token = default)
        {
            // Create the tool and open the port
            // The device starts with 115200 baud.
            var espTool = new ESPTool();
            espTool.OpenSerial("COM30", 115200);

            // Starting the bootloader will return a bootloader object.
            // This bootloader is used by the tools to execute commands.
            var bootLoader = await espTool.GetBootloaderTool().StartAsync(token);

            // For example, detect the connected chip type.
            var chipType = await espTool.GetChipDetectTool(bootLoader).DetectChipTypeAsync(token);

            // Since the bootloader doenst support all commands, we need to start the softloader.
            // Similar as before, this returns a softloader object.
            // The tool will automatically load the right softloader depending on the chiptype.
            var softLoader = await espTool.GetSoftloaderTool(bootLoader, chipType).StartAsync(token);

            // Optionally, you can increase the baudrate for faster uploading.
            // The comport will also automatically change to the new baudrate.
            await espTool.GetChangeBaudrateTool(softLoader).ChangeBaudAsync(baudRate, token);
        }

        public async Task UploadFirmware(CancellationToken token = default)
        {
            _ = softloader ?? throw new Exception("Initialize first");

            // First, load the firmware to memory.
            // Its also possible to create a custom IFirmwareProvider implementation.
            var myFirmware = new FirmwareProvider(
                entryPoint: 0x00000000,
                segments:
                [
                    new FirmwareSegmentProvider(0x00001000, File.ReadAllBytes("bootloader.bin")),
                    new FirmwareSegmentProvider(0x00008000, File.ReadAllBytes("partition-table.bin")),
                    new FirmwareSegmentProvider(0x0000F000, File.ReadAllBytes("ota_data_initial.bin")),
                    new FirmwareSegmentProvider(0x00800000, File.ReadAllBytes("application.bin")),
                ]
            );

            // Instantiate a tool to upload to RAM
            IUploadTool uploadToolRam = espTool.GetUploadRamTool(softloader, chipType);

            // Instantiate a tool to upload to FLASH
            IUploadTool uploadToolFlash = espTool.GetUploadFlashTool(softloader, chipType);

            // Instantiate a tool to upload to FLASH, with a gzip deflated for faster uploading.
            // The tool handles the compression, so pass the firmware uncompressed in the next step.
            IUploadTool uploadToolFlashDeflated = espTool.GetUploadFlashDeflatedTool(softloader, chipType);

            // Create the FirmwareUploadTool, use one of the upload tools from before
            var firmwareTool = espTool.GetFirmwareUploadTool(uploadToolFlashDeflated);

            // Some tools can report progress.
            firmwareTool.Progress = new Progress<float>(p =>
            {
                Debug.WriteLine($"Progress: {p * 100}%");
            });

            // Upload the firmware
            await firmwareTool.UploadFirmwareAsync(myFirmware, token);

            // Reset the device
            await espTool.GetResetDeviceTool().ResetAsync(token);
        }

        public async Task EraseFlash(CancellationToken token = default)
        {
            _ = softloader ?? throw new Exception("Initialize first");

            // Erase the flash
            await espTool.GetFlashEraseTool(softloader).EraseFlashAsync(token);

            // Reset the device
            await espTool.GetResetDeviceTool().ResetAsync(token);
        }
    }
}

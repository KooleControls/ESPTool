using EspDotNet.Tools.Firmware;
using EspDotNet;
using System.Diagnostics;
using System.Threading;
using System;

public class Example
{

    public async Task UploadFirmwareAsync(CancellationToken token = default)
    { 
        var toolbox = new ESPToolbox();
        var communicator = toolbox.CreateCommunicator();
        toolbox.OpenSerial(communicator, "COM30", 115200);

        var loader = await toolbox.StartBootloaderAsync(communicator);
        var chipType = await toolbox.DetectChipTypeAsync(loader);

        var softloader = await toolbox.StartSoftloaderAsync(communicator, loader, chipType);
        await toolbox.ChangeBaudAsync(communicator, softloader, 921600);

        var uploadTool = toolbox.CreateUploadFlashDeflatedTool(softloader, chipType);
        var myFirmware = GetFirmware();
        var progress = new Progress<float>(p => Debug.WriteLine($"Upload progress: {p:P0}"));

        await toolbox.UploadFirmwareAsync(uploadTool, myFirmware, token, progress);
        await toolbox.ResetDeviceAsync(communicator);
    }

    public async Task ReadFlashAsync(CancellationToken token = default)
    {
        var toolbox = new ESPToolbox();
        var communicator = toolbox.CreateCommunicator();
        toolbox.OpenSerial(communicator, "COM30", 115200);

        var loader = await toolbox.StartBootloaderAsync(communicator);
        var chipType = await toolbox.DetectChipTypeAsync(loader);

        var softloader = await toolbox.StartSoftloaderAsync(communicator, loader, chipType);
        await toolbox.ChangeBaudAsync(communicator, softloader, 921600);

        var readTool = toolbox.CreateReadFlashTool(softloader, communicator, chipType);
        var progress = new Progress<float>(p => Debug.WriteLine($"Read progress: {p:P0}"));

        // Read 4KB from flash starting at address 0x1000
        var flashData = await toolbox.ReadFlashAsync(readTool, 0x1000, 4096, token, progress);
        Debug.WriteLine($"Read {flashData.Length} bytes from flash");

        // Or read directly to a file
        await readTool.ReadFlashToFileAsync(0x1000, 4096, "flash_dump.bin", token);
    }


    private IFirmwareProvider GetFirmware()
    {
        return new FirmwareProvider(
            entryPoint: 0x00000000,
            segments:
            [
                new FirmwareSegmentProvider(0x00001000, File.ReadAllBytes("bootloader.bin")),
                new FirmwareSegmentProvider(0x00008000, File.ReadAllBytes("partition-table.bin")),
                new FirmwareSegmentProvider(0x0000F000, File.ReadAllBytes("ota_data_initial.bin")),
                new FirmwareSegmentProvider(0x00800000, File.ReadAllBytes("application.bin")),
            ]
        );
    }

}

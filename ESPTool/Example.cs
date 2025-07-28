using EspDotNet.Tools.Firmware;
using EspDotNet;
using System.Diagnostics;
using System.Threading;
using System;
using System.IO;

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

        var readTool = toolbox.CreateReadFlashTool(communicator, softloader, chipType);
        var progress = new Progress<float>(p => Debug.WriteLine($"Read progress: {p:P0}"));

        // Option 1: Read directly to a file using a FileStream
        using (var fileStream = new FileStream("flash_dump.bin", FileMode.Create, FileAccess.Write))
        {
            await toolbox.ReadFlashAsync(readTool, 0x1000, 4096, fileStream, token, progress);
        }

        // Option 2: Read to memory using a MemoryStream with MD5 verification
        using (var memoryStream = new MemoryStream())
        {
            await toolbox.ReadFlashAsync(readTool, 0x1000, 4096, memoryStream, true, token, progress);
            var flashData = memoryStream.ToArray();
            Debug.WriteLine($"Read {flashData.Length} bytes from flash (MD5 verified)");
        }

        // Option 3: Use convenience method for in-memory reading (backwards compatibility)
        var flashData2 = await toolbox.ReadFlashAsync(readTool, 0x1000, 4096, token, progress);
        Debug.WriteLine($"Read {flashData2.Length} bytes from flash");

        // Option 4: Use convenience method for file writing (backwards compatibility)
        await toolbox.ReadFlashToFileAsync(readTool, 0x1000, 4096, "flash_dump2.bin", token, progress);
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

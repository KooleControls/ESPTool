# ESPTool - ESP32 Flashing and Bootloader Tool

**ESPTool** is a C# library designed to interact with ESP32 devices for tasks such as flashing firmware and erasing flash memory. This library provides a way to communicate with the ESP32 using serial communication.

Currently, the library supports basic ESP32 operations like flashing and erasing firmware.

(Outdated, todo, fix cicd, see [#](https://github.com/KooleControls/ESPTool/issues/5))
[![NuGet version (ESPTool)](https://img.shields.io/nuget/v/ESPTool)](https://www.nuget.org/packages/ESPTool/)

> **Looking for a GUI tool?** Check out the [ESPFlasher GUI tool on GitHub](https://github.com/KooleControls/ESPFlasher).

## Features
- Start the ESP32 bootloader
- Load and run a softloader
- Erase flash memory
- Upload firmware to the device
- Detect chip type
- Reset the device

## Example

```csharp
// Create the tool
var espTool = new ESPTool();

// Open the serial port
espTool.OpenSerial("COM3", 115200);

// Start the bootloader (This is the bootloader on the device itself)
await espTool.StartBootloaderAsync();

// Detect the chip type
var chipType = await espTool.DetectChipTypeAsync();
Console.WriteLine($"Detected chip: {chipType}");

// Get the firmware for the softloader, this one is sepecific to the ESP32
var softloaderFirmware = DefaultFirmwareProviders.ESP32_Softloader;

// Start the softloader (This supports commands that the bootloader lacks)
await espTool.StartSoftloaderAsync(softloaderFirmware);

// Erase the flash, this is not supported by the bootloader, so we needed to start the softloader
await espTool.EraseFlashAsync();

// Upload some firmware, you will need to implement the IFirmwareProvider interface
var firmware = xxx;
IProgress<float> progress = new Progress<float>(p => Console.WriteLine($"Progress: {p * 100:F2}%"));
await espTool.UploadFirmwareAsync(firmware, progress: progress);

// Reset the device after uploading firmware
await espTool.ResetDeviceAsync();

// Close the comport
espTool.CloseSerial();
```

## Configuration
ESPTool can be initialized with a custom configuration:
```csharp
var config = new ESPToolConfig
{
    BootloaderSequence = new List<PinSequenceItem> { ... },
    ResetSequence = new List<PinSequenceItem> { ... },
    Devices = new List<DeviceConfig>
    {
        new DeviceConfig { ChipType = ChipTypes.ESP32, RamBlockSize = 4096 }
    }
};

var espTool = new ESPTool(config);
```

## Loader and SoftLoader

`Loader` is the base class that interacts with the bootloader running on the ESP device, handling memory and flash operations. `SoftLoader` extends this functionality by enabling compressed data flashing and dynamic baud rate adjustments. The `SoftLoader` must be uploaded to the ESP device's RAM before use.

## Communicator

`Communicator` handles low-level serial communication with the ESP32 device, including operations like opening and closing the serial port, sending SLIP-encoded frames, and reading data from the device.

## License
This project is licensed under the MIT License. See LICENSE for details.

## Additional Resources

- Official ESPTool Protocol Documentation: [Espressif Docs](https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html)


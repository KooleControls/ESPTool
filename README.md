# ESPTool - ESP32 Flashing and Bootloader Tool

**ESPTool** is a C# implementation of Espressif's ESP tool: [https://github.com/espressif/esptool](https://github.com/espressif/esptool)

I created this project because I wanted a native C# implementation that does not rely on external applications. This library is designed to interact with ESP32 devices for tasks such as flashing firmware and erasing flash memory, providing a way to communicate with ESP32 using serial communication.


[![NuGet](https://img.shields.io/nuget/v/ESPTool.svg)](https://www.nuget.org/packages/ESPTool)


> **Looking for a GUI tool?** Check out the [ESPFlasher GUI tool on GitHub](https://github.com/KooleControls/ESPFlasher).

## Features

- Execute pin sequences to start the bootloader
- Load and run a softloader
- Erase flash memory
- Upload firmware to the device
- Detect chip type
- Reset the device

## Supported Devices

The code should support all ESP devices, however I dont have these devices available for testing. The devices that have been tested are:

- [ ] ESP8266,
- [x] ESP32,
- [ ] ESP32-c2,
- [x] ESP32-c3,
- [ ] ESP32-c6,
- [ ] ESP32-h2,
- [ ] ESP32-p4,
- [ ] ESP32-s2,
- [x] ESP32-s3,
- [ ] ESP32-c6beta,
- [ ] ESP32-h2beta1,
- [ ] ESP32-h2beta2,
- [ ] ESP32-s3beta2,

## Example Usage

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

// Get the firmware for the softloader
var softloaderFirmware = DefaultFirmwareProviders.GetSoftloaderForDevice(chipType);

// Start the softloader (This supports commands that the bootloader lacks)
await espTool.StartSoftloaderAsync(softloaderFirmware);

// Erase the flash, this is not supported by the bootloader, so we needed to start the softloader
await espTool.EraseFlashAsync();

// Change the baudrate for faster uploading
await _espTool.ChangeBaudAsync(BaudRate, token);

// Upload some firmware, you will need to implement the IFirmwareProvider interface
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
await _espTool.UploadFirmwareAsync(myFirmware, FirmwareUploadMethods.FlashDeflated);

// Reset the device after uploading firmware
await espTool.ResetDeviceAsync();

// Close the serial port
espTool.CloseSerial();
```


# Software Overview

`ESPTool` is a C# implementation of the ESP32 flashing and bootloader communication protocol. It allows you to interface with ESP32-based devices over serial communication, perform chip detection, erase flash memory, and upload firmware. It extends the default bootloader functionality by enabling the use of a **softloader**, which adds extra commands that aren't available in the ESP ROM bootloader.

### Loader and SoftLoader

The `ESP32BootLoader` and `SoftLoader` classes are responsible for communicating with the bootloader and softloader running on the ESP device. 

- **`ESP32BootLoader`**: Represents the interaction with the bootloader built into the ESP ROM, offering limited commands.
- **`SoftLoader`**: Represents the interaction with a software-loaded bootloader running in RAM, extending functionality with additional commands like compressed data flashing and baud rate adjustments.

### Communicator

- **`Communicator`** handles low-level serial communication with the ESP32 device, including:
  - Opening and closing the serial port
  - Sending SLIP-encoded frames
  - Reading and writing data from/to the device
  - Sending bootloader and reset pin sequences

## License

This project is licensed under the MIT License. See LICENSE for details.

## Additional Resources

- **Official ESPTool Protocol Documentation:** [Espressif Docs](https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html)
- **Stubloaders** can be found in the toolchain: \tools\python_env\idf5.0_py3.11_env\Lib\site-packages\esptool\targets\stub_flasher

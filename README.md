# ESPTool Library

**ESPTool** is a C# library designed to interact with ESP32 devices for tasks such as flashing firmware, erasing flash memory, syncing with the bootloader, and managing device communications. This library provides a structured way to communicate with the ESP32 using serial communication and custom loaders.

Currently, the library supports basic ESP32 operations like flashing and erasing firmware.

(Outdated, todo, fix cicd, see [#](https://github.com/KooleControls/ESPTool/issues/5))
[![NuGet version (ESPTool)](https://img.shields.io/nuget/v/ESPTool)](https://www.nuget.org/packages/ESPTool/)

## Key Concepts

### 1. **DeviceManager**

The `DeviceManager` class serves as the top-level interface for interacting with ESP32 devices. It manages the initialization, syncing with the bootloader, detection of chip types, and communication with the ESP32 device using the `Loader` and `SoftLoader` classes.

### 2. **ESP32Device**

`ESP32Device` is the specific implementation of the device logic tailored for ESP32 devices. It handles flashing operations, changing baud rates, and managing interactions with the softloader.

## Components Overview

### DeviceManager

`DeviceManager` is responsible for managing the connection and communication with the ESP32 device. It abstracts away the lower-level details and provides high-level methods for:
- Initializing the device
- Syncing with the bootloader
- Detecting the chip type (ESP32)
- Uploading a softloader
- Managing the baud rate

**Usage Example**:
```csharp
ILoggerFactory loggerFactory = new LoggerFactory();
var deviceManager = new DeviceManager(loggerFactory);

var device = await deviceManager.InitializeAsync("COM3", 115200);
if (device is ESP32Device esp32Device)
{
    await esp32Device.StartSoftloaderAsync();  // Start the softloader
    await esp32Device.EraseFlashAsync();       // Erase the flash memory
}
```

### ESP32Device

`ESP32Device` implements the logic for interacting with ESP32 devices specifically. It provides methods to:
- **Start the Softloader**: A RAM-based softloader is used to enable faster and more advanced flashing operations.
- **Change Baud Rate**: Modify the baud rate for communication.
- **Erase Flash**: Completely erase the flash memory on the ESP32 device.
- **Upload Firmware**: Supports both compressed and uncompressed firmware uploads.

#### Key Methods:
- `StartSoftloaderAsync()`: Uploads and starts the softloader in RAM.
- `ChangeBaudAsync(int baud)`: Changes the baud rate of the device.
- `EraseFlashAsync()`: Erases the entire flash memory.
- `UploadToFlashAsync(Stream data, uint offset)`: Uploads a stream of uncompressed data to the flash.
- `UploadCompressedToFlashAsync(Stream data, uint offset)`: Uploads compressed data to the flash.

### Loader and SoftLoader

`Loader` is the base class that interacts with the bootloader running on the ESP device, handling memory and flash operations. `SoftLoader` extends this functionality by enabling compressed data flashing and dynamic baud rate adjustments. The `SoftLoader` must be uploaded to the ESP device's RAM before use.

### Communicator

`Communicator` handles low-level serial communication with the ESP32 device, including operations like opening and closing the serial port, sending SLIP-encoded frames, and reading data from the device.

## How to Use

### Uploading Firmware to the ESP32

The following example shows how to initialize the ESP32 device, upload a softloader, and flash firmware.

```csharp
ILoggerFactory loggerFactory = new LoggerFactory();
var deviceManager = new DeviceManager(loggerFactory);

// Initialize the ESP32 device
var device = await deviceManager.InitializeAsync("COM3", 115200);

if (device is ESP32Device esp32Device)
{
    // Start the softloader
    await esp32Device.StartSoftloaderAsync();

    // Erase the flash memory
    await esp32Device.EraseFlashAsync();

    // Flash firmware
    using (FileStream firmwareStream = File.OpenRead("firmware.bin"))
    {
        await esp32Device.UploadToFlashAsync(firmwareStream, 0x1000); // Offset 0x1000
    }

    // Reset the device after flashing
    await esp32Device.ResetDeviceAsync();
}
```

## Additional Resources

- Official ESPTool Protocol Documentation: [Espressif Docs](https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html)

# ESPTool - ESP32 Flashing and Bootloader Tool

**ESPTool** is a native C# implementation of Espressif's ESP tool ([esptool](https://github.com/espressif/esptool)). This library was created to enable direct interaction with ESP devices (such as ESP32) without relying on external applications. It provides a rich set of tools for flashing firmware, erasing flash memory, detecting chip types, and managing bootloader and softloader communication—all via serial communication.

[![NuGet](https://img.shields.io/nuget/v/ESPTool.svg)](https://www.nuget.org/packages/ESPTool)

> **Looking for a GUI tool?** Check out the [ESPFlasher GUI tool on GitHub](https://github.com/KooleControls/ESPFlasher).

## Features

- **Bootloader and Softloader Communication:**  
  - Execute pin sequences to start the bootloader.
  - Load and run a softloader.
- **Firmware Operations:**  
  - Upload firmware using dedicated tools (Flash, Flash Deflated, RAM).
  - Erase flash memory.
- **Device Management:**  
  - Detect chip type.
  - Reset the device.

## Supported Devices

While the code is designed to support all ESP devices, testing has only been performed on a subset. Devices tested include:

- [ ] ESP8266  
- [x] ESP32  
- [ ] ESP32-c2  
- [x] ESP32-c3  
- [ ] ESP32-c6  
- [ ] ESP32-h2  
- [ ] ESP32-p4  
- [ ] ESP32-s2  
- [x] ESP32-s3  
- [ ] ESP32-c6beta  
- [ ] ESP32-h2beta1  
- [ ] ESP32-h2beta2  
- [ ] ESP32-s3beta2  

## Architecture Overview

The library is organized into several self‑contained tools, each responsible for a specific aspect of ESP device communication and firmware handling:

### Loader Tools

- **`BootloaderTool`**  
  Initiates communication with the built‑in ROM bootloader on the ESP device.
- **`SoftloaderTool`**  
  Handles the process of uploading a softloader (stubloader) into RAM, which extends functionality with commands not available in the ROM bootloader.

### Firmware Upload Tools

Each upload tool encapsulates a specific firmware upload mechanism:
- **`UploadRamTool`** – For uploading firmware directly into RAM.
- **`UploadFlashTool`** – For flashing firmware to the device’s flash memory.
- **`UploadFlashDeflatedTool`** – For flashing compressed (deflated) firmware images.
- **`FirmwareUploadTool`** – Wraps around an upload tool and manages firmware segmented transfers with progress reporting.

### Additional Tools

- **`ChipTypeDetectTool`** – Detects the chip type of the connected device.
- **`ChangeBaudrateTool`** – Handles baud rate changes.
- **`FlashEraseTool`** – Erases the device’s flash memory.
- **`ResetDeviceTool`** – Resets the device by executing a reset pin sequence.

## Example

The main `ESPTool` class serves as a toolbox for creating and retrieving the various tools needed to interact with an ESP device. Its design forces the user to explicitly handle the device state by passing the appropriate loader and chip type when needed, reducing the risk of operating on a disconnected or unsupported device.

For usage examples, see [Example.cs](./ESPTool/Example.cs).

## License

This project is licensed under the MIT License. See [LICENSE](./LICENSE) for details.

## Additional Resources

- **Official ESPTool Protocol Documentation:** [Espressif Docs](https://docs.espressif.com/projects/esptool/en/latest/esp32/advanced-topics/serial-protocol.html)
- **Stubloaders Location:**  
  Typically found in the toolchain under:  
  `\tools\python_env\idf5.0_py3.11_env\Lib\site-packages\esptool\targets\stub_flasher`


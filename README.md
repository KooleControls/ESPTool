# ESPTool
Created the Espressif ESPTool in native C#

This tool is still under development. Right now only support for flashing and erasing the ESP32 is added.

[![NuGet version (ESPTool)](https://img.shields.io/nuget/v/ESPTool)](https://www.nuget.org/packages/ESPTool/)




## Some documentation
Altough the idea is to add more documentation, right now I lack the time to create a neath wiki. For now this will have to do.



The library consists of 4 main components:
- ESPTool, Allows user to flash or erase a device. The general idea is that the ESPTool has the same functionality as the esptool created by espressif. 

- Device, This contains some basic functionality in order to figure out what device is connected. Once this is known, a new device should be created in order to use the right implementation. For example: An instance of 'Device' is created. This instance is used to open the serial connection, enter bootloader, sync with device, and read register 0x40001000. Now the acutal connected device is known, so an instance that represents the connected device can be created. For example, an instance of the ESP32 is created and used from now on. More devices can be supported by creating classes that inherit the 'Device' class. Methods that need a different implementation can be overridden in order to achive compatibility.

- Loader, The loader implements the functions that are supported by the bootloader that runs on the device. By default only the basic functions are supported. Once the softloader is uploaded and started, the loader can be changed to the object representing the new softloader. When a custom bootloader is used, the library can be extended by implementing a new class that inherits from loader. Certain methods can be overloaded in order to communicate with the custom bootloader in the device.

- Communicator, The Communicator implements the slip framing protocol and the serial communication.




# ESPTool
Created the Espressif ESPTool in native C#

This tool is still under development. Right now only support for flashing and erasing the ESP32 is added.


## Some documentation
Altough the idea is to add more documentation, right now I lack the time to create a neath wiki. For now this will have to do.


The topology of the libary is as follows:


[ESP tool] <-> abstraction functions <-> [Device] <-> Commands / result <-> [Loader] <-> Frames <-> [Communicator]



ESPTool			- Allows user to flash or erase a device.

Device			- Represents a device like the ESP32 or any other for that matter.
				- Overrides sertain methods in order to create an abstraction layer.

Loader			- Contains functionality that is supported by the loader. This can be the ROM loader or the software loader.
				- Right now only the default loader and softloader are supported.
				- Overrides sertain methods in order to create an abstraction layer.

Communicator	- Uses slipframing to communicate with the device. 







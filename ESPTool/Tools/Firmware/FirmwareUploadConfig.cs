namespace EspDotNet.Tools.Firmware
{
    public class FirmwareUploadConfig
    {
        public uint BlockSize { get; set; } = 1024;
        public FirmwareUploadMethods UploadMethod { get; set; }
        public bool ExecuteAfterSending { get; set; } = false;
    }




}

using System.Buffers.Text;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EspDotNet.Tools.Firmware
{
    public class DefaultFirmwareProviders
    {
        public static IFirmwareProvider GetSoftloaderForDevice(ChipTypes chipType)
        {
            return chipType switch
            {
                ChipTypes.ESP32c6beta => GetFirmware("stub_flasher_32c6beta.json"),
                ChipTypes.ESP32h2 => GetFirmware("stub_flasher_32h2.json"),
                ChipTypes.ESP32h2beta1 => GetFirmware("stub_flasher_32h2beta1.json"),
                ChipTypes.ESP32h2beta2 => GetFirmware("stub_flasher_32h2beta2.json"),
                ChipTypes.ESP32p4 => GetFirmware("stub_flasher_32p4.json"),
                ChipTypes.ESP32s2 => GetFirmware("stub_flasher_32s2.json"),
                ChipTypes.ESP32s3 => GetFirmware("stub_flasher_32s3.json"),
                ChipTypes.ESP32s3beta2 => GetFirmware("stub_flasher_32s3beta2.json"),
                ChipTypes.ESP8266 => GetFirmware("stub_flasher_8266.json"),
                ChipTypes.ESP32 => GetFirmware("stub_flasher_32.json"),
                ChipTypes.ESP32c2 => GetFirmware("stub_flasher_32c2.json"),
                ChipTypes.ESP32c3 => GetFirmware("stub_flasher_32c3.json"),
                ChipTypes.ESP32c6 => GetFirmware("stub_flasher_32c6.json"),
                _ => throw new Exception($"Chip type {chipType} is not supported"),
            };
        }

        private static FirmwareProvider GetFirmware(string resourceName)
        {
            var stub = GetStub(resourceName);

            return new FirmwareProvider(
                entryPoint: stub.Entry,
                segments: new List<IFirmwareSegmentProvider>
                {
                    new FirmwareSegmentProvider(stub.TextStart, Convert.FromBase64String(stub.Text)),
                    new FirmwareSegmentProvider(stub.DataStart, Convert.FromBase64String(stub.Data)),
                }
            );
        }

        private static Stub GetStub(string resourceName)
        {
            string json = ReadEmbeddedJson(resourceName);
            return JsonSerializer.Deserialize<Stub>(json)
                ?? throw new Exception($"Failed to deserialize JSON from resource: {resourceName}");
        }

        private static string ReadEmbeddedJson(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"{assembly.GetName().Name}.Resources.stub.{resourceName}";

            using Stream stream = assembly.GetManifestResourceStream(resourcePath)
                ?? throw new Exception($"Embedded resource '{resourceName}' not found in assembly.");

            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        class Stub
        {
            [JsonPropertyName("entry")]
            public uint Entry { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("text_start")]
            public uint TextStart { get; set; }

            [JsonPropertyName("data")]
            public string Data { get; set; } = string.Empty;

            [JsonPropertyName("data_start")]
            public uint DataStart { get; set; }

            [JsonPropertyName("bss_start")]
            public uint BssStart { get; set; }
        }
    }
}

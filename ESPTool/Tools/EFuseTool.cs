using EspDotNet.Config;
using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;

namespace EspDotNet.Tools
{
    public class EFuseTool
    {
        private readonly ILoader _loader;
        private readonly DeviceConfig _deviceConfig;
        public EFuseTool(ILoader loader, DeviceConfig deviceConfig)
        {
            _loader = loader;
            _deviceConfig = deviceConfig;
        }

        public async Task<byte[]> ReadAsync(EFlagKey key, CancellationToken token)
        {
            if (!_deviceConfig.EFlags.TryGetValue(key, out EFuseMapping? mapping))
            {
                throw new Exception($"EFuse mapping not found for key {key}");
            }

            // Calculate how many 4-byte registers we need to read.
            int registersNeeded = (mapping.Size + 3) / 4;
            List<byte> resultBytes = new List<byte>();

            for (int i = 0; i < registersNeeded; i++)
            {
                uint address = mapping.Address + (uint)(i * 4);
                uint regValue = await _loader.ReadRegisterAsync(address, token);
                // Convert the register value to bytes (assuming little-endian format).
                byte[] regBytes = BitConverter.GetBytes(regValue);
                resultBytes.AddRange(regBytes);
            }

            // Trim the result to the requested size.
            byte[] data = resultBytes.Take(mapping.Size).ToArray();
            return data;
        }
    }
}

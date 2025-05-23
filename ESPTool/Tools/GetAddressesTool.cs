﻿namespace EspDotNet.Tools
{

    public class GetAddressesTool
    {

        private readonly ILoader _loader;

        public GetAddressesTool(ILoader loader)
        {
            _loader = loader;
        }

        // Useful notes for how I derived these addresses
        //EFUSE_BASE = 0x60007000  # BLOCK0 read base address
        //https://github.com/espressif/esptool/blob/955943a8ab02fde8113d67f9ebab75ba4cbaa9f0/esptool/targets/esp32s3.py
        //MAC_EFUSE_REG = EFUSE_BASE + 0x044 ( 0x60007000 + 0x044 = 60007044)
        //MAC_EFUSE_BASE_ADDR = (MAC_EFUSE_REG + 4 (0x60007044 + 4 = 60007048)

        // Getting the first 4 octets
        public async Task<uint> GetBaseMacAsync(CancellationToken token)
        {
            uint EMAC_BASE_ADDR = 0x60007048;
            uint BaseAddr = await _loader.ReadRegisterAsync(EMAC_BASE_ADDR, token);
            return BaseAddr;
        }

        // Getting the last 6 octets
        public async Task<uint> GetLastSixOctetsAsync(CancellationToken token)
        {
            uint MAC_EFUSE_REG = 0x60007044;
            uint maccy = await _loader.ReadRegisterAsync(MAC_EFUSE_REG, token);
            return maccy;
        }

        // Gets the base Mac Address
        public async Task<string> GetBaseMacAddressAsync(CancellationToken token)
        {
            string BaseMacAddress = string.Empty;

            // Getting Base part of Mac address
            uint BaseAddr = await GetBaseMacAsync(token);
            string BaseMac = BaseAddr.ToString("X");

            //WiFi Base MacAddress
            uint maccy = await GetLastSixOctetsAsync(token);
            string LowerMac = maccy.ToString("X");

            BaseMacAddress = SplitAndFormat(BaseMac + LowerMac);

            return BaseMacAddress;
        }

        // Gets the wifi  AP address
        public async Task<string> GetWiFiAPMacAddressAsync(CancellationToken token)
        {
            string WifiAPAddr = string.Empty;

            // Getting Base part of Mac address
            uint BaseAddr = await GetBaseMacAsync(token);
            string BaseMac = BaseAddr.ToString("X");

            // WiFiAP = Base Mac Address + 1
            uint maccy = (await GetLastSixOctetsAsync(token) + 1);
            string LowerMac = maccy.ToString("X");

            WifiAPAddr = SplitAndFormat(BaseMac + LowerMac);

            return WifiAPAddr;
        }

        public async Task<string> GetBlueToothMacAddressAsync(CancellationToken token)
        {
            string BTAddr = string.Empty;
            // Getting Base part of Mac address
            uint BaseAddr = await GetBaseMacAsync(token);
            string BaseMac = BaseAddr.ToString("X");

            // BlueTooth = Base Mac Address + 2
            uint maccy = (await GetLastSixOctetsAsync(token) + 2);
            string LowerMac = maccy.ToString("X");

            BTAddr = SplitAndFormat(BaseMac + LowerMac);

            return BTAddr;
        }

        // Gets the Ethernet Address of the device
        public async Task<string> GetEthernetAddressAsync(CancellationToken token)
        {
            string EthernetAddr = string.Empty;
            // Getting Base part of Mac address
            uint BaseAddr = await GetBaseMacAsync(token);
            string BaseMac = BaseAddr.ToString("X");

            // Ethernet = Base Mac Address + 3
            uint maccy = (await GetLastSixOctetsAsync(token) + 3);
            string LowerMac = maccy.ToString("X");

            EthernetAddr = SplitAndFormat(BaseMac + LowerMac);

            return EthernetAddr;
        }

        // String formatter to format the address to be more like a mac address
        public static string SplitAndFormat(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < input.Length; i += 2)
            {
                if (i + 1 < input.Length)
                {
                    result.Append(input.Substring(i, 2));
                    if (i + 2 < input.Length)
                    {
                        result.Append(":");
                    }
                }
                else
                {
                    result.Append(input.Substring(i)); // Handle odd length
                }
            }
            return result.ToString();
        }
    }
}

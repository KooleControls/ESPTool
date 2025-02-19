﻿using System.Buffers.Text;
using System.Text;

namespace EspDotNet.Tools.Firmware
{
    public class DefaultFirmwareProviders
    {
        public static IFirmwareProvider ESP32_Softloader { get; } = new FirmwareProvider(
            entryPoint: 0x400BE5AC,
            segments: new List<IFirmwareSegmentProvider>
            {
                new FirmwareSegmentProvider(0x3FFDEBA8, Convert.FromBase64String("CMD8Pw==")),
                new FirmwareSegmentProvider(0x400BE000, Convert.FromBase64String("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"))
            }
        );

        public static IFirmwareProvider ESP32S2_Softloader { get; } = new FirmwareProvider(
            entryPoint: 0x40378A80,
            segments: new List<IFirmwareSegmentProvider>
            {
                new FirmwareSegmentProvider(0x3FCB2BFC, Convert.FromBase64String("CMD8Pw==")),
                new FirmwareSegmentProvider(0x40378000, Convert.FromBase64String("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"))
            }
        );
    }
}

using ESPTool.Firmware;
using ESPTool.Loaders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Devices
{
    
    public class ESP32 : Device
    {
        static readonly UInt32 ESP_RAM_BLOCK = 0x1800;
        static readonly UInt32 FLASH_WRITE_SIZE = 0x400;

        public ESP32(Device dev) : base(dev)
        {
            Loader = new ESP32Loader(Loader);
        }

        public async Task<bool> UploadToRAM(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken))
        {
            bool suc = true;
            foreach (Segment di in firmware.Segments)
            {
                if (suc)
                {
                    UInt32 size = (UInt32)di.Data.Length;
                    UInt32 blockSize = ESP_RAM_BLOCK;
                    UInt32 blocks = size / blockSize;

                    if (size % blockSize != 0)
                        blocks++;

                    suc = (await Loader.MEM_BEGIN(size, blocks, blockSize, di.Offset, ct)).Success;
                    for (UInt32 i = 0; i < blocks && suc; i++)
                    {
                        UInt32 srcInd = i * blockSize;
                        UInt32 len = (UInt32)size - srcInd;
                        if (len > blockSize)
                            len = blockSize;
                        byte[] buffer = di.Data.SubArray((int)srcInd, (int)len);

                        suc &= (await Loader.MEM_DATA(buffer, i, ct)).Success;
                    }
                }
            }

            if (suc)
            {
                suc &= (await Loader.MEM_END((UInt32)(execute ? 0 : 1), firmware.EntryPoint, ct)).Success;
            }

            return suc;
        }

        public async Task<bool> UploadToFLASH(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken))
        {
            bool suc = true;
            foreach (Segment di in firmware.Segments)
            {
                if (suc)
                {
                    UInt32 size = (UInt32)di.Data.Length;
                    UInt32 blockSize = FLASH_WRITE_SIZE;
                    UInt32 blocks = size / blockSize;

                    if (size % blockSize != 0)
                        blocks++;

                    suc = (await Loader.FLASH_BEGIN(size, blocks, blockSize, di.Offset, ct)).Success;
                    for (UInt32 i = 0; i < blocks && suc; i++)
                    {
                        UInt32 srcInd = i * blockSize;
                        UInt32 len = (UInt32)size - srcInd;
                        if (len > blockSize)
                            len = blockSize;
                        byte[] buffer = di.Data.SubArray((int)srcInd, (int)len);

                        suc &= (await Loader.FLASH_DATA(buffer, i, ct)).Success;
                    }
                }
            }

            if (suc)
            {
                suc &= (await Loader.MEM_END((UInt32)(execute?0:1), firmware.EntryPoint, ct)).Success;
            }

            return suc;
        }

        public async Task<bool> StartStubloader(CancellationToken ct = default(CancellationToken))
        {
            FirmwareImage stub = StubLoaders.ESP32;

            var ohai = Loader.WaitForOHAI(ct);
            bool suc = await UploadToRAM(stub, true, ct);

            if (suc) suc &= await ohai;

            if(suc)
            {
                Loader = new SoftLoader(Loader);
            }

            return suc;
        }
    }
}

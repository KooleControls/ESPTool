using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Devices
{
    public class ESP32 : Device
    {
        static readonly UInt32 ESP_RAM_BLOCK = 0x1800;

        public ESP32(Device dev) : base(dev)
        {

        }


        public async Task<bool> StartStubloader(CancellationToken ct = default(CancellationToken))
        {
            StubLoader stub = StubLoader.ESP32;

            bool suc = true;
            foreach (DataItem di in stub.DataItems)
            {
                if (suc)
                {
                    UInt32 size = (UInt32)di.Data.Length;
                    UInt32 blockSize = ESP_RAM_BLOCK;
                    UInt32 blocks = size / blockSize;

                    if (size % blockSize != 0)
                        blocks++;

                    suc = (await Loader.MEM_BEGIN(size, blocks, blockSize, di.Address, ct)).Success;
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
                var ohai = Loader.WaitForOHAI(ct);
                suc &= (await Loader.MEM_END(0, stub.EntryPoint, ct)).Success;
                if (suc)
                {
                    suc &= await ohai;
                }
            }
            return suc;
        }
    }

}

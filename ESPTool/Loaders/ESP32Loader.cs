using ESPTool.CMD;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class ESP32Loader : Loader
    {

        public ESP32Loader(Loader lod) : base(lod)
        {

        }
        public override async Task<ReplyCMD> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default(CancellationToken))
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(baud),
                BitConverter.GetBytes(0)));
            return await DoFrame(request, ct);
        }
    }
}


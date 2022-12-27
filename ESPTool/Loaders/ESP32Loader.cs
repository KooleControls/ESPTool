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
        public override async Task<Result> ChangeBaud(int baud, int oldBaud, CancellationToken ct = default)
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(baud),
                BitConverter.GetBytes(0)));
            return ToResult( await DoFrame(request, ct));
        }
    }
}


using ESPTool.CMD;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class ESP32Loader : Loader
    {
        public async Task<ReplyCMD> CHANGE_BAUDRATE(uint newBaud, CancellationToken ct)
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(newBaud),
                BitConverter.GetBytes(0)));
            return await DoFrame(request, ct);
        }
    }
}


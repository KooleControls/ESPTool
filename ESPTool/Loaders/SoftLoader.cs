using ESPTool.CMD;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Loaders
{
    public class SoftLoader : Loader
    {

        public SoftLoader(Loader lod) : base(lod)
        {

        }


        public async Task<ReplyCMD> CHANGE_BAUDRATE(uint newBaud, uint oldBaud, CancellationToken ct)
        {
            RequestCMD request = new RequestCMD(0x0f, false, Helpers.Concat(
                BitConverter.GetBytes(newBaud),
                BitConverter.GetBytes(oldBaud)));
            return await DoFrame(request, ct);
        }
    }
}


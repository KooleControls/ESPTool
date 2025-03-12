using EspDotNet.Communication;
using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class ChangeBaudrateTool
    {
        private readonly ILoader _loader;
        private readonly Communicator _communicator;
        public ChangeBaudrateTool(Communicator communicator, ILoader loader)
        {
            _loader = loader;
            _communicator = communicator;
        }
        public async Task ChangeBaudAsync(int baud, CancellationToken token)
        {
            int oldBaud = _communicator.GetBaudRate();
            if (baud == oldBaud)
                return;
            await _loader.ChangeBaudAsync(baud, oldBaud, token);
            _communicator.ChangeBaudRate(baud);

        }
    }
}

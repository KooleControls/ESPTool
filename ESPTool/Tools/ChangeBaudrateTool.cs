using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class ChangeBaudrateTool
    {
        private readonly ILoader _loader;
        public ChangeBaudrateTool(ILoader loader)
        {
            _loader = loader;
        }
        public async Task ChangeBaudAsync(int baud, int oldBaud, CancellationToken token)
        {
            await _loader.ChangeBaudAsync(baud, oldBaud, token);
        }
    }
}

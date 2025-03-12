using EspDotNet.Loaders;
using EspDotNet.Loaders.SoftLoader;

namespace EspDotNet.Tools
{
    public class FlashEraseTool
    {
        private readonly SoftLoader _loader;
        public FlashEraseTool(SoftLoader loader)
        {
            _loader = loader;
        }
        public async Task EraseFlashAsync(CancellationToken token)
        {
            await _loader.EraseFlashAsync(token);
        }
    }
}

using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class FlashEraseTool
    {
        private readonly ILoader _loader;
        public FlashEraseTool(ILoader loader)
        {
            _loader = loader;
        }
        public async Task EraseFlashAsync(CancellationToken token)
        {
            await _loader.EraseFlashAsync(token);
        }
    }
}

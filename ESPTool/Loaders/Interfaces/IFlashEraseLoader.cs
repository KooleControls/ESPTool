namespace ESPTool.Loaders.Interfaces
{
    public interface IFlashEraseLoader : ILoader
    {
        Task EraseFlashAsync(CancellationToken token);
    }
}


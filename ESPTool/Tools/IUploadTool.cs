namespace EspDotNet.Tools
{
    public interface IUploadTool
    {
        public IProgress<float> Progress { get; set; }
        Task Upload(Stream data, uint offset, uint size, CancellationToken token);
        Task UploadAndExecute(Stream uncompressedData, uint offset, uint unCompressedSize, uint entryPoint, CancellationToken token);
    }
}

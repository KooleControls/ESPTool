using EspDotNet.Loaders;
using EspDotNet.Tools.Firmware;
using EspDotNet.Utils;
using System;

namespace EspDotNet.Tools
{
    public class FirmwareUploadTool
    {
        public IProgress<float> Progress { get; set; } = new Progress<float>();

        private readonly IUploadTool _uploadTool;

        public FirmwareUploadTool(IUploadTool uploadTool)
        {
            _uploadTool = uploadTool;
        }

        public Task UploadFirmwareAsync(IFirmwareProvider firmwareProvider, CancellationToken token)
        {
            return ProcessSegmentsAsync(firmwareProvider, executeOnLast: false, token);
        }

        public Task UploadFirmwareAndExecuteAsync(IFirmwareProvider firmwareProvider, CancellationToken token)
        {
            return ProcessSegmentsAsync(firmwareProvider, executeOnLast: true, token);
        }

        private async Task ProcessSegmentsAsync(IFirmwareProvider firmwareProvider, bool executeOnLast, CancellationToken token)
        {
            Progress.Report(0);
            var segments = firmwareProvider.Segments.ToList();
            var totalSize = segments.Sum(s => s.Size);
            float progressAccumulated = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var dataStream = await segment.GetStreamAsync(token);
                var offset = segment.Offset;
                var size = segment.Size;
                float segmentWeight = (float)size / totalSize; // Fractional contribution of this segment

                // Update the upload tool's progress handler for the current segment.
                _uploadTool.Progress = new Progress<float>(p =>
                {
                    float segmentProgress = p * segmentWeight;
                    Progress.Report(segmentProgress + progressAccumulated);
                });

                bool isLastSegment = (i == segments.Count - 1);
                if (executeOnLast && isLastSegment)
                {
                    await _uploadTool.UploadAndExecute(dataStream, offset, size, firmwareProvider.EntryPoint, token);
                }
                else
                {
                    await _uploadTool.Upload(dataStream, offset, size, token);
                }

                progressAccumulated += segmentWeight;
            }
            Progress.Report(1);
        }
    }

}

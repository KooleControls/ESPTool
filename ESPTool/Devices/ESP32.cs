using ESPTool.CMD;
using ESPTool.Firmware;
using ESPTool.Loaders;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Devices
{
    
    public class ESP32 : Device
    {
        static readonly int ESP_RAM_BLOCK = 0x1800;
        static readonly int FLASH_WRITE_SIZE = 0x400;

        public ESP32(Device dev) : base(dev)
        {
            Loader = new ESP32Loader(Loader);

        }

        public override async Task<Result> UploadToRAM(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken))
        {
            Result result = Result.OK;
            foreach (Segment di in firmware.Segments)
            {
                if (result.Success)
                {
                    UInt32 size = (UInt32)di.Data.Length;
                    UInt32 blockSize = (UInt32)ESP_RAM_BLOCK;
                    UInt32 blocks = size / blockSize;

                    if (size % blockSize != 0)
                        blocks++;

                    result = await Loader.MEM_BEGIN(size, blocks, blockSize, di.Offset, ct);
                    for (UInt32 i = 0; i < blocks && result.Success; i++)
                    {
                        UInt32 srcInd = i * blockSize;
                        UInt32 len = (UInt32)size - srcInd;
                        if (len > blockSize)
                            len = blockSize;
                        byte[] buffer = di.Data.SubArray((int)srcInd, (int)len);

                        result = await Loader.MEM_DATA(buffer, i, ct);
                    }
                }
            }

            if (result.Success)
            {
                result = await Loader.MEM_END((UInt32)(execute ? 0 : 1), firmware.EntryPoint, ct);
            }

            return result;
        }

        public override async Task<Result> UploadToFLASH(FirmwareImage firmware, bool execute, CancellationToken ct = default, IProgress<float> progress = default)
        {
            Result result = Result.OK;
            int written = 0;
            int totalSize = 0;
            foreach (Segment seg in firmware.Segments)
                totalSize += seg.Data.Length;
            float prevProg = 0;
            foreach (Segment segment in firmware.Segments)
            {
                if (result.Success)
                {
                    UInt32 size = (UInt32)segment.Data.Length;
                    UInt32 blockSize = (UInt32)FLASH_WRITE_SIZE;
                    UInt32 blocks = size / blockSize;

                    if (size % blockSize != 0)
                        blocks++;

                    result = await Loader.FLASH_BEGIN(size, blocks, blockSize, segment.Offset, ct);
                    for (UInt32 i = 0; i < blocks && result.Success; i++)
                    {
                        UInt32 srcInd = i * blockSize;
                        UInt32 len = (UInt32)size - srcInd;
                        if (len > blockSize)
                            len = blockSize;
                        byte[] buffer = segment.Data.SubArray((int)srcInd, (int)len);

                        result = await Loader.FLASH_DATA(buffer, i, ct);

                        written += (int)len;
                        float prog = (float)written / (float)totalSize;
                        if((prog - prevProg) > 0.01)
                        {
                            progress?.Report(prog);
                            prevProg = prog;
                        }
                    }

                    if (result.Success)
                    {
                        byte[] sourceHash;
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            md5.TransformFinalBlock(segment.Data, 0, segment.Data.Length);
                            sourceHash = md5.Hash;
                        }
                        Result<byte[]> md5Result = await Loader.SPI_FLASH_MD5(segment.Offset, (UInt32)segment.Data.Length);
                        result = md5Result;

                        if (md5Result.Success)
                        {
                            if (sourceHash.SequenceEqual(md5Result.Value))
                                result = Result.OK;
                            else
                                result = new Result { Success = false, Error = Errors.MD5Mismatch };
                        }
                    }
                }
            }

            if (result.Success)
            {
                result = await Loader.FLASH_END((UInt32)(execute?0:1), firmware.EntryPoint, ct);
            }

            return result;
        }

        //3550
        public override async Task<Result> UploadToFLASHDeflated(FirmwareImage firmware, bool execute, CancellationToken ct = default(CancellationToken), IProgress<float> progress = default)
        {
            Result result = Result.OK;
            int written = 0;
            int totalSize = 0;
            foreach (Segment seg in firmware.Segments)
            {
                byte[] cImg = Helpers.Compress(seg.Data);
                totalSize += cImg.Length;
            }

            float prevProg = 0;

            for(int seg=0; seg<firmware.Segments.Count && result.Success; seg++)
            {
                Segment segment = firmware.Segments[seg];
                byte[] uImg = segment.Data;
                byte[] cImg = Helpers.Compress(uImg);

                int uSize = uImg.Length;
                int cSize = cImg.Length;

                int numBlocks = (cSize + FLASH_WRITE_SIZE - 1) / FLASH_WRITE_SIZE;
                int eraseBlocks = (uSize + FLASH_WRITE_SIZE - 1) / FLASH_WRITE_SIZE;

                result = await Loader.FLASH_DEFL_BEGIN((UInt32)uSize, (UInt32)numBlocks, (UInt32)FLASH_WRITE_SIZE, segment.Offset, ct);

                for (int i = 0; i < numBlocks && result.Success; i++)
                {
                    byte[] cBlock = cImg.SubArray(i * FLASH_WRITE_SIZE, FLASH_WRITE_SIZE, (byte)0xFF);
                    result = await Loader.FLASH_DEFL_DATA(cBlock, (UInt32)i, ct);
                    
                    written += FLASH_WRITE_SIZE;
                    float prog = (float)written / (float)totalSize;
                    if ((prog - prevProg) > 0.01)
                    {
                        progress?.Report(prog);
                        prevProg = prog;
                    }
                }

                if(result.Success && Loader is SoftLoader)  //Not the most beautiful solution but it will have to do for now.
                {
                    // Stub only writes each block to flash after 'ack'ing the receive, so do a final dummy operation which won't be 'ack'ed until the last block has actually been written out to flash
                    result = await Loader.READ_REG(0x40001000, ct);
                }

                
                if (result.Success)
                {
                    byte[] sourceHash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        md5.TransformFinalBlock(segment.Data, 0, segment.Data.Length);
                        sourceHash = md5.Hash;
                    }
                    Result<byte[]> md5Result = await Loader.SPI_FLASH_MD5(segment.Offset, (UInt32)segment.Data.Length);
                    result = md5Result;

                    if (md5Result.Success)
                    {
                        if (sourceHash.SequenceEqual(md5Result.Value))
                            result = Result.OK;
                        else
                            result = new Result { Success = false, Error = Errors.MD5Mismatch };
                    }
                }
            }


            if (result.Success)
            {
                result = await Loader.FLASH_DEFL_END((UInt32)(execute ? 0 : 1), firmware.EntryPoint, ct);
            }

            return result;


        }
        

        public override async Task<Result> StartStubloader(CancellationToken ct = default(CancellationToken))
        {
            FirmwareImage stub = StubLoaders.ESP32;

            var ohai = Loader.WaitForOHAI(ct);
            Result result = await UploadToRAM(stub, true, ct);

            if (result.Success)
                result = await ohai;

            if(result.Success)
            {
                Loader = new SoftLoader(Loader);
            }

            return result;
        }

        public override async Task<Result> EraseFlash(CancellationToken ct = default(CancellationToken))
        {
            Result result = await Loader.ERASE_FLASH(ct);
            return result;
        }
    }
}

using System.IO.Compression;

namespace EspDotNet.Utils
{

    public class ZlibCompressionHelper
    {
        public static void CompressToZlibStream(Stream inputStream, Stream compressedStream)
        {
            // Write the zlib header (0x78, 0x9C for default compression)
            compressedStream.WriteByte(0x78);
            compressedStream.WriteByte(0x9C);

            // Use DeflateStream to compress the data (without zlib header/footer)
            using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                inputStream.CopyTo(deflateStream);
            }

            // Calculate the zlib footer (Adler-32 checksum) for the compressed data
            byte[] adler32Checksum = CalculateAdler32Checksum(inputStream);
            compressedStream.Write(adler32Checksum, 0, adler32Checksum.Length);
        }

        // Method to calculate Adler-32 checksum
        private static byte[] CalculateAdler32Checksum(Stream stream)
        {
            const uint MOD_ADLER = 65521;
            long position = stream.Position;
            stream.Position = 0; // Reset stream position

            uint a = 1, b = 0;

            int currentByte;
            while ((currentByte = stream.ReadByte()) != -1)
            {
                a = (a + (uint)currentByte) % MOD_ADLER;
                b = (b + a) % MOD_ADLER;
            }

            stream.Position = position; // Restore stream position

            uint checksum = b << 16 | a;

            byte[] result = new byte[4];
            result[0] = (byte)(checksum >> 24 & 0xFF);
            result[1] = (byte)(checksum >> 16 & 0xFF);
            result[2] = (byte)(checksum >> 8 & 0xFF);
            result[3] = (byte)(checksum & 0xFF);

            return result;
        }
    }

}

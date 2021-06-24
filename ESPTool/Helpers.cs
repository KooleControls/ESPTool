using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using zlib;

namespace ESPTool
{
    public static class Helpers
    {

        public static T[] SubArray<T>(this T[] data, int index, int length, T padding = default)
        {
            T[] result = new T[length];
            int len = Math.Min(length, data.Length - index);
            Array.Copy(data, index, result, 0, len);
            for (int i = data.Length; i < length; i++)
                result[i] = padding;
            return result;
        }

        /// <summary>
        /// Creates a copy of this array.
        /// </summary>
        /// <typeparam name="T">Type of items in array</typeparam>
        /// <param name="data">The array to copy</param>
        /// <param name="index">Start index</param>
        /// <returns>A partial copy of the origional</returns>
        public static T[] SubArray<T>(this T[] data, int index)
        {
            T[] result = new T[data.Length - index];
            Array.Copy(data, index, result, 0, data.Length - index);
            return result;
        }

        public static T[] Copy<T>(this T[] data)
        {
            T[] result = new T[data.Length];
            Array.Copy(data, 0, result, 0, data.Length);
            return result;
        }

        public static T[] Append<T>(this T[] array1, params T[][] arrays)
        {
            foreach (T[] array2 in arrays)
            {
                int array1OriginalLength = array1.Length;
                Array.Resize<T>(ref array1, array1OriginalLength + array2.Length);
                Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
            }

            return array1;
        }

        public static T[] Concat<T>(params T[][] ts)
        {
            T[] result = new T[0];
            foreach (var v in ts)
                result = result.Append(v);
            return result;
        }

        
        //https://stackoverflow.com/questions/1014005/how-to-populate-instantiate-a-c-sharp-array-with-a-single-value
        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        public static string GetCurrentMethodName([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        public static UInt32 ReadUInt32(this Stream stream)
        {
            byte[] data = new byte[4];
            stream.Read(data, 0, data.Length);
            return BitConverter.ToUInt32(data, 0);
        }

        public static byte[] Compress(byte[] data)
        {
            byte[] compressedBytes;

            CompressData(data, out compressedBytes);
            return compressedBytes;


            using (var uncompressedStream = new MemoryStream(data))
            {
                using (var compressedStream = new MemoryStream())
                {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new GZipStream(compressedStream, CompressionLevel.Optimal, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return compressedBytes;
        }


        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }
}

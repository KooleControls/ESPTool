using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ESPTool
{
    public static class Helpers
    {
        /// <summary>
        /// Creates a copy of this array.
        /// </summary>
        /// <typeparam name="T">Type of items in array</typeparam>
        /// <param name="data">The array to copy</param>
        /// <param name="index">Start index</param>
        /// <param name="length">Number of items to be copied</param>
        /// <returns>A partial copy of the origional</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
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
    }
}

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Shared.Helper
{
    public static class Utilities
    {
        /// <summary>
        ///     Convert a byte array to an Object.
        ///     Copied from http://snippets.dzone.com/posts/show/3897
        /// </summary>
        /// <param name="arrBytes">
        ///     The byte[] array to be converted.
        /// </param>
        /// <returns>
        ///     The object to which the byte array is converted.
        /// </returns>
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            var obj = binForm.Deserialize(memStream);

            return obj;
        }
    }
}
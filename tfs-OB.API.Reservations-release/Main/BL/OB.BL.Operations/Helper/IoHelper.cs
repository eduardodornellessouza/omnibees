using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    /// <summary>
    /// Helper to convert stream to string or string to stream
    /// </summary>
    public static class IoHelper
    {
        public static Stream GetStream(string stringData)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringData);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetString(Stream inputStream)
        {
            string output;

            // Always read from start
            inputStream.Seek(0, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(inputStream))
            {
                output = reader.ReadToEnd();
            }

            return output;
        }

        public static void WriteStream(Stream inputStream, ref byte[] dataBytes)
        {
            using (Stream outputStream = inputStream)
            {
                outputStream.Write(dataBytes, 0, dataBytes.Length);
            }
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] GetBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static T Clone<T>(this T source)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, source);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)serializer.ReadObject(ms);
            }
        }
    }
}

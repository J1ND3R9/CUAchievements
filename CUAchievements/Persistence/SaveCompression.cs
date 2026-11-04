using System.IO;
using System.IO.Compression;
using System.Text;

namespace CUAchievements.Persistence
{
    internal static class SaveCompression
    {
        public static byte[] Compress(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            using MemoryStream output = new();
            using (GZipStream gzip = new(output, CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        public static string Decompress(byte[] data)
        {
            using MemoryStream input = new(data);
            using GZipStream gzip = new(input, CompressionMode.Decompress);
            using StreamReader reader = new(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}

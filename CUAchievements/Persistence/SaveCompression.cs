namespace CUAchievements.Persistence
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    internal static class SaveCompression
    {
        public static byte[] Compress(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        public static string Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Text;

public static class ZipUtils
{
    public static byte[] CompressString(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream())
        {
            using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionLevel.Optimal))
            {
                msi.CopyTo(gs);
            }
            return mso.ToArray();
        }
    }

    public static string ToBase64(string json)
    {
        var compressed = CompressString(json);
        return Convert.ToBase64String(compressed);
    }
    
    public static string FromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream())
        {
            using (var gs = new System.IO.Compression.GZipStream(msi, System.IO.Compression.CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }
            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }
}
namespace ReviewDen;

public static class Images
{
    public const int MaxBytes = 5 * 1024 * 1024;

    // ponytail: JPEG only (browser re-encodes every upload to JPEG first), add ImageSharp if other formats must be accepted server-side
    // Returns the JPEG with EXIF/XMP/IPTC/comment segments removed (GPS lives there), or null if it is not a well-formed JPEG header.
    public static byte[]? CleanJpeg(byte[] d)
    {
        if (d.Length < 4 || d[0] != 0xFF || d[1] != 0xD8) return null;
        using var o = new MemoryStream(d.Length);
        o.Write(d, 0, 2);
        var i = 2;
        while (i + 4 <= d.Length)
        {
            if (d[i] != 0xFF) return null;
            var m = d[i + 1];
            if (m == 0xFF) { i++; continue; }
            if (m == 0xDA) { o.Write(d, i, d.Length - i); return o.ToArray(); }
            var len = (d[i + 2] << 8) | d[i + 3];
            if (len < 2 || i + 2 + len > d.Length) return null;
            var isMeta = m == 0xE1 || (m >= 0xE3 && m <= 0xEF) || m == 0xFE;
            if (!isMeta) o.Write(d, i, 2 + len);
            i += 2 + len;
        }
        return null;
    }

    static void Check(bool ok, string msg) { if (!ok) throw new Exception("selftest failed: " + msg); }

    public static void SelfTest()
    {
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE1, 0, 6, (byte)'G', (byte)'P', (byte)'S', 0, 0xFF, 0xDB, 0, 3, 7, 0xFF, 0xDA, 0, 2, 1, 2, 0xFF, 0xD9];
        var clean = CleanJpeg(jpeg)!;
        Check(clean.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB, 0, 3, 7, 0xFF, 0xDA, 0, 2, 1, 2, 0xFF, 0xD9 }), "EXIF not stripped");
        Check(CleanJpeg([0x89, 0x50, 0x4E, 0x47]) == null, "PNG accepted");
        Check(CleanJpeg([0xFF, 0xD8, 0xFF, 0xE1, 0x7F, 0xFF]) == null, "truncated segment accepted");

        var now = DateTime.UtcNow;
        var fresh = new Post { Likes = 5, CreatedAt = now.AddHours(-1) };
        var old = new Post { Likes = 50, CreatedAt = now.AddDays(-7) };
        Check(fresh.Score(now) > old.Score(now), "trending ignores age");
        Console.WriteLine("selftest ok");
    }
}

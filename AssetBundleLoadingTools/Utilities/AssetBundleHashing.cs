using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Windows;

namespace AssetBundleLoadingTools.Utilities
{
    // Not ideal because this will read files/bundles twice
    // This could probably be fixed by patching all the AssetBundle.LoadFromX methods
    // This class shouldn't even be necessary (you'd think Unity would have an easily accessible way to check the hashes of AssetBundles...)
    public static class AssetBundleHashing
    {
        // From SongCore
        public static string? FromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(path);
            var hashBytes = sha256.ComputeHash(stream);
            return ByteToHexBitFiddle(hashBytes);
        }

        public static string FromBytes(byte[] bytes)
        {
            // Use input string to calculate MD5 hash
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(bytes);

            return ByteToHexBitFiddle(hashBytes);
        }

        // Black magic https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/14333437#14333437
        static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
    }
}

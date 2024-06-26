﻿using System.Security.Cryptography;
using System.Text;

namespace FortLibrary.Encoders
{
    public class Hex
    {
        public static string GenerateRandomHexString(int length)
        {
            byte[] RandomBytes = new byte[length / 2];
            new Random().NextBytes(RandomBytes);
            return BitConverter.ToString(RandomBytes).Replace("-", string.Empty);
        }

        public static string MakeHexWithString(string hexString)
        {
            byte[] data = Encoding.UTF8.GetBytes(hexString);
            string hexHash = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(data);
                hexHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            return hexHash;
        }

        public static string MakeHexWithString2(string hexString)
        {
            byte[] data = Encoding.UTF8.GetBytes(hexString);
            string hexHash = "";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(data);
                hexHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            return hexHash;
        }
    }
}

﻿using System.Security.Cryptography;

namespace GlobalPlatform.NET.SecureChannel.Cryptography
{
    internal static class SecureRandom
    {
        public static byte[] GetBytes(int length)
        {
            var data = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            return data;
        }
    }
}

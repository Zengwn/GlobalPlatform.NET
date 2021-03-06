﻿using System;
using System.Collections.Generic;
using System.Linq;
using GlobalPlatform.NET.Tools;

namespace GlobalPlatform.NET.Extensions
{
    internal static class ByteExtensions
    {
        /// <summary>
        /// Adds a range of bytes to the collection, prefixed by a single byte denoting the range's length. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static byte AddRangeWithLength(this ICollection<byte> bytes, byte[] range)
        {
            byte length = range.LengthChecked();

            bytes.Add(length);
            bytes.AddRange(range);

            return length;
        }

        /// <summary>
        /// Adds TLV-encoded data to the collection. 
        /// </summary>
        /// <returns></returns>
        public static void AddTLV(this ICollection<byte> bytes, TLV tlv) => bytes.AddRange(tlv.Data);

        /// <summary>
        /// Returns the length of the array, as a checked byte. 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte LengthChecked(this IEnumerable<byte> array) => checked((byte)array.Count());

        /// <summary>
        /// Pads a byte array using ISO/IEC 7816-4. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IList<byte> Pad(this IList<byte> bytes)
        {
            bytes.Add(0x80);

            if (bytes.Count % 8 != 0)
            {
                bytes.AddRange(Enumerable.Repeat<byte>(0x00, 8 - bytes.Count % 8));
            }

            return bytes;
        }

        /// <summary>
        /// Checks if a specific bit within a byte is set to 1.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static bool IsBitSet(this byte b, byte bit)
        {
            if (bit > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(b), "The bit to check must be between 0-7 inclusive.");
            }

            byte mask = (byte)(1 << bit);

            return (b & mask) == mask;
        }
    }
}

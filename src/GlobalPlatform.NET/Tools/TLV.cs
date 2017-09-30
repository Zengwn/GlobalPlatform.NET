﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalPlatform.NET.Tools
{
    /// <summary>
    /// Builds and parses data encoded using ASN.1 BER-TLV. 
    /// </summary>
    public class TLV
    {
        private TLV()
        {
        }

        private IList<byte> tag;
        private IList<byte> value;
        private readonly List<TLV> nestedTags = new List<TLV>();

        public IEnumerable<byte> Tag => this.tag;

        public int Length => this.Value.Count;

        public IList<byte> Value => IsTagConstructed(this.Tag) ? this.NestedTags.SelectMany(x => x.Data).ToList() : this.value.ToList();

        public IReadOnlyCollection<TLV> NestedTags => this.nestedTags;

        public IList<byte> Data
        {
            get
            {
                var data = new List<byte>();

                data.AddRange(this.Tag);

                if (this.Length < 128)
                {
                    data.Add(checked((byte)this.Length));
                }
                else
                {
                    byte[] lengthBytes = BitConverter.GetBytes(this.Length);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBytes);
                    }

                    while (lengthBytes.First() == 0x00)
                    {
                        lengthBytes = lengthBytes.Skip(1).ToArray();
                    }

                    data.Add(checked((byte)(lengthBytes.Length ^ 0b10000000)));

                    data.AddRange(lengthBytes);
                }

                data.AddRange(this.Value);

                return data;
            }
        }

        private static bool IsTagConstructed(byte tag) => IsTagConstructed(new[] { tag });

        private static bool IsTagConstructed(IEnumerable<byte> tag) => (tag.First() & 0b00100000) > 0;

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static TLV Build(byte tag) => Build(new[] { tag });

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>

        public static TLV Build(IEnumerable<byte> tag) => new TLV
        {
            tag = tag.ToList(),
            value = new List<byte>()
        };

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TLV Build(byte tag, params byte[] value) => Build(new[] { tag }, value);

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        public static TLV Build(IEnumerable<byte> tag, params byte[] value) => new TLV
        {
            tag = tag.ToList(),
            value = value
        };

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="nestedTags"></param>
        /// <returns></returns>

        public static TLV Build(byte tag, params TLV[] nestedTags) => Build(new[] { tag }, nestedTags);

        /// <summary>
        /// Encodes a block of data using ASN.1 BER-TLV. Length is encoded as definite. 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="nestedTags"></param>
        /// <returns></returns>

        public static TLV Build(IEnumerable<byte> tag, params TLV[] nestedTags)
        {
            if (!IsTagConstructed(tag.ToArray()))
            {
                throw new ArgumentException("A primitive tag may not contain TLV-encoded data.");
            }

            var tlv = new TLV
            {
                tag = tag.ToList()
            };

            tlv.nestedTags.AddRange(nestedTags);

            return tlv;
        }

        /// <summary>
        /// Parses a block of data encoded using ASN.1 BER-TLV. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ICollection<TLV> Parse(IList<byte> data)
        {
            var collection = new List<TLV>();

            for (int i = 0, start = 0; i < data.Count; start = i)
            {
                bool isConstructed = IsTagConstructed(data[i]);
                bool hasMoreBytes = (data[i] & 0b00011111) == 0b00011111;

                while (hasMoreBytes && (data[++i] & 0b10000000) > 0) { }

                i++;

                var tag = data.Skip(start).Take(i - start);

                if (data[i] == 0b10000000)
                {
                    throw new NotSupportedException("Indefinite lengths are not supported.");
                }

                bool hasShortLength = (data[i] & 0b10000000) == 0;

                int length = 0;

                if (hasShortLength)
                {
                    length = data[i];
                }
                else
                {
                    int numLengthBytes = data[i] & 0b01111111;

                    if (numLengthBytes > 4)
                    {
                        throw new NotSupportedException("Unable to parse length values exceeding 4 octets.");
                    }

                    var lengthBytes = data.Skip(i + 1).Take(numLengthBytes).ToList();

                    length = lengthBytes.Aggregate(length, (current, lengthByte) => (current << 8) | lengthByte);
                }

                i = hasShortLength ? i + 1 : i + (data[i] & 0b01111111) + 1;

                var value = data.Skip(i).Take(length).ToList();

                i += length;

                TLV tlv;

                if (isConstructed)
                {
                    tlv = new TLV
                    {
                        tag = tag.ToArray()
                    };

                    tlv.nestedTags.AddRange(Parse(value));
                }
                else
                {
                    tlv = new TLV
                    {
                        tag = tag.ToArray(),
                        value = value
                    };
                }

                collection.Add(tlv);
            }

            return collection;
        }
    }
}

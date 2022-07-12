﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OrleansURLShortener
{
    /// <summary>
    /// <para>
    ///     MurmurHash3 x64 128-bit variant.
    /// </para>
    /// <para>
    ///     See https://github.com/aappleby/smhasher/wiki/MurmurHash3 for more information. Port of 
    ///     https://github.com/aappleby/smhasher/blob/61a0530f28277f2e850bfc39600ce61d02b518de/src/MurmurHash3.cpp#L255
    /// </para>
    /// </summary>
    public class MurmurHash3
    {

        #region Hash32
        /// <summary>MurmurHash3 32-bit implementation for strings.</summary>
        /// <param name="value">The string to hash.</param>
        /// <param name="seed">The seed to initialize with.</param>
        /// <returns>The 32-bit hash.</returns>
        public static uint Hash32(string value, uint seed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int size = Encoding.UTF8.GetMaxByteCount(value.Length);
            Span<byte> span = size <= 256 ? stackalloc byte[size] : new byte[size];
            int len = Encoding.UTF8.GetBytes(value, span);
            return MurmurHash3.Hash32(span.Slice(0, len), seed);
        }

        /// <summary>MurmurHash3 32-bit implementation for booleans.</summary>
        /// <param name="value">The data to hash.</param>
        /// <param name="seed">The seed to initialize with.</param>
        /// <returns>The 32-bit hash.</returns>
        public static uint Hash32(bool value, uint seed)
        {
            // Ensure that a bool is ALWAYS a single byte encoding with 1 for true and 0 for false.
            return MurmurHash3.Hash32((byte)(value ? 1 : 0), seed);
        }

        /// <summary>MurmurHash3 32-bit implementation.</summary>
        /// <param name="value">The data to hash.</param>
        /// <param name="seed">The seed to initialize with.</param>
        /// <returns>The 32-bit hash.</returns>
        public static unsafe uint Hash32<T>(T value, uint seed)
            where T : unmanaged
        {
            ReadOnlySpan<T> span = new ReadOnlySpan<T>(&value, 1);
            return MurmurHash3.Hash32(MemoryMarshal.AsBytes(span), seed);
        }

        /// <summary>MurmurHash3 32-bit implementation.</summary>
        /// <param name="span">The data to hash.</param>
        /// <param name="seed">The seed to initialize with.</param>
        /// <returns>The 32-bit hash.</returns>
        public static unsafe uint Hash32(ReadOnlySpan<byte> span, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new InvalidOperationException("Host machine needs to be little endian.");
            }

            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = seed;

            // body
            unchecked
            {
                fixed (byte* bytes = span)
                {
                    for (int i = 0; i < span.Length - 3; i += 4)
                    {
                        uint k1 = *(uint*)(bytes + i);

                        k1 *= c1;
                        k1 = MurmurHash3.RotateLeft32(k1, 15);
                        k1 *= c2;

                        h1 ^= k1;
                        h1 = MurmurHash3.RotateLeft32(h1, 13);
                        h1 = (h1 * 5) + 0xe6546b64;
                    }

                    {
                        // tail
                        uint k = 0;

                        switch (span.Length & 3)
                        {
                            case 3:
                                k ^= (uint)bytes[span.Length - 1] << 16;
                                k ^= (uint)bytes[span.Length - 2] << 8;
                                k ^= (uint)bytes[span.Length - 3];
                                break;

                            case 2:
                                k ^= (uint)bytes[span.Length - 1] << 8;
                                k ^= (uint)bytes[span.Length - 2];
                                break;

                            case 1:
                                k ^= (uint)bytes[span.Length - 1];
                                break;
                        }

                        k *= c1;
                        k = MurmurHash3.RotateLeft32(k, 15);
                        k *= c2;
                        h1 ^= k;
                    }
                }

                // finalization
                h1 ^= (uint)span.Length;
                h1 ^= h1 >> 16;
                h1 *= 0x85ebca6b;
                h1 ^= h1 >> 13;
                h1 *= 0xc2b2ae35;
                h1 ^= h1 >> 16;
            }

            return h1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft32(uint n, int numBits)
        {
            if (numBits >= 32)
            {
                throw new ArgumentOutOfRangeException($"{nameof(numBits)} must be less than 32");
            }

            return (n << numBits) | (n >> (32 - numBits));
        }
        #endregion


        /// <summary>Computes the hash value for the specified byte array.</summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null.</exception>
        public byte[] ComputeHash(byte[] buffer)
        {
            return ComputeHash(buffer, 0, buffer.Length);
        }


        /// <summary>Computes the hash value for the specified region of the specified byte array.</summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash code.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="count" /> is an invalid value.-or-<paramref name="buffer" /> length is invalid.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is out of range. This parameter requires a non-negative number.</exception>
        public unsafe byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            const ulong c1 = 0x87c37b91114253d5;
            const ulong c2 = 0x4cf5ad432745937f;

            int nblocks = count / 16;

            ulong h1 = 0;
            ulong h2 = 0;

            // body
            unchecked
            {
                fixed (byte* pbuffer = buffer)
                {
                    byte* pinput = pbuffer + offset;
                    ulong* body = (ulong*)pinput;

                    ulong k1;
                    ulong k2;

                    for (int i = 0; i < nblocks; i++)
                    {
                        k1 = body[i * 2];
                        k2 = body[i * 2 + 1];

                        k1 *= c1;
                        k1 = (k1 << 31) | (k1 >> (64 - 31)); // ROTL64(k1, 31);
                        k1 *= c2;
                        h1 ^= k1;

                        h1 = (h1 << 27) | (h1 >> (64 - 27)); // ROTL64(h1, 27);
                        h1 += h2;
                        h1 = h1 * 5 + 0x52dce729;

                        k2 *= c2;
                        k2 = (k2 << 33) | (k2 >> (64 - 33)); // ROTL64(k2, 33);
                        k2 *= c1;
                        h2 ^= k2;

                        h2 = (h2 << 31) | (h2 >> (64 - 31)); // ROTL64(h2, 31);
                        h2 += h1;
                        h2 = h2 * 5 + 0x38495ab5;
                    }

                    // tail

                    k1 = 0;
                    k2 = 0;

                    byte* tail = pinput + nblocks * 16;
                    switch (count & 15)
                    {
                        case 15:
                            k2 ^= (ulong)tail[14] << 48;
                            goto case 14;
                        case 14:
                            k2 ^= (ulong)tail[13] << 40;
                            goto case 13;
                        case 13:
                            k2 ^= (ulong)tail[12] << 32;
                            goto case 12;
                        case 12:
                            k2 ^= (ulong)tail[11] << 24;
                            goto case 11;
                        case 11:
                            k2 ^= (ulong)tail[10] << 16;
                            goto case 10;
                        case 10:
                            k2 ^= (ulong)tail[9] << 8;
                            goto case 9;
                        case 9:
                            k2 ^= tail[8];
                            k2 *= c2;
                            k2 = (k2 << 33) | (k2 >> (64 - 33)); // ROTL64(k2, 33);
                            k2 *= c1;
                            h2 ^= k2;
                            goto case 8;
                        case 8:
                            k1 ^= (ulong)tail[7] << 56;
                            goto case 7;
                        case 7:
                            k1 ^= (ulong)tail[6] << 48;
                            goto case 6;
                        case 6:
                            k1 ^= (ulong)tail[5] << 40;
                            goto case 5;
                        case 5:
                            k1 ^= (ulong)tail[4] << 32;
                            goto case 4;
                        case 4:
                            k1 ^= (ulong)tail[3] << 24;
                            goto case 3;
                        case 3:
                            k1 ^= (ulong)tail[2] << 16;
                            goto case 2;
                        case 2:
                            k1 ^= (ulong)tail[1] << 8;
                            goto case 1;
                        case 1:
                            k1 ^= tail[0];
                            k1 *= c1;
                            k1 = (k1 << 31) | (k1 >> (64 - 31)); // ROTL64(k1, 31);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                    }
                }
            }

            // finalization
            h1 ^= (ulong)count;
            h2 ^= (ulong)count;

            h1 += h2;
            h2 += h1;

            h1 = FMix64(h1);
            h2 = FMix64(h2);

            h1 += h2;
            h2 += h1;

            var ret = new byte[16];
            fixed (byte* pret = ret)
            {
                var ulpret = (ulong*)pret;

                ulpret[0] = Reverse(h1);
                ulpret[1] = Reverse(h2);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong FMix64(ulong k)
        {
            unchecked
            {
                k ^= k >> 33;
                k *= 0xff51afd7ed558ccd;
                k ^= k >> 33;
                k *= 0xc4ceb9fe1a85ec53;
                k ^= k >> 33;
                return k;
            }
        }

        private static ulong Reverse(ulong value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
    }
}

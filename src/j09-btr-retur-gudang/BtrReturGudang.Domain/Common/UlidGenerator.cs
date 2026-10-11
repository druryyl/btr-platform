using System;
using System.Security.Cryptography;
using System.Text;

namespace BtrReturGudang.Domain.Common
{
    /// <summary>
    /// Generates canonical 26-character Crockford Base32 ULIDs (Universally Unique Lexicographically Sortable Identifiers).
    /// Traceable to: IR-RO-01, TD-004, Ulid 1.2.6 standard compatibility.
    ///
    /// Canonical layout:
    /// - 48-bit timestamp (milliseconds since Unix Epoch)
    /// - 80-bit randomness (with monotonic increment if within same millisecond)
    /// - Encoded as 26 Crockford Base32 characters (0123456789ABCDEFGHJKMNPQRSTVWXYZ)
    /// </summary>
    public static class UlidGenerator
    {
        private const string CrockfordBase32Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private const int UlidStringLength = 26;

        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();
        private static readonly object SyncLock = new object();

        private static long _lastTimestampMs = -1L;
        private static readonly byte[] LastEntropy = new byte[10];

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Generates a new canonical 26-character monotonic ULID string.
        /// </summary>
        public static string NewUlid()
        {
            var nowMs = (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
            return NewUlid(nowMs);
        }

        /// <summary>
        /// Generates a 26-character ULID string for a specific timestamp (milliseconds since Unix epoch).
        /// </summary>
        public static string NewUlid(long timestampMs)
        {
            var bytes = new byte[16];

            lock (SyncLock)
            {
                if (timestampMs <= _lastTimestampMs)
                {
                    // Monotonic increment of last entropy
                    timestampMs = _lastTimestampMs;
                    IncrementEntropy(LastEntropy);
                }
                else
                {
                    _lastTimestampMs = timestampMs;
                    Rng.GetBytes(LastEntropy);
                }

                // 48-bit timestamp (6 bytes big-endian)
                bytes[0] = (byte)((timestampMs >> 40) & 0xFF);
                bytes[1] = (byte)((timestampMs >> 32) & 0xFF);
                bytes[2] = (byte)((timestampMs >> 24) & 0xFF);
                bytes[3] = (byte)((timestampMs >> 16) & 0xFF);
                bytes[4] = (byte)((timestampMs >> 8) & 0xFF);
                bytes[5] = (byte)(timestampMs & 0xFF);

                // 80-bit entropy (10 bytes)
                Buffer.BlockCopy(LastEntropy, 0, bytes, 6, 10);
            }

            return Encode(bytes);
        }

        private static void IncrementEntropy(byte[] entropy)
        {
            for (var i = entropy.Length - 1; i >= 0; i--)
            {
                if (entropy[i] < 0xFF)
                {
                    entropy[i]++;
                    return;
                }
                entropy[i] = 0;
            }
        }

        /// <summary>
        /// Encodes 16 raw bytes into 26 Crockford Base32 characters.
        /// </summary>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 16)
                throw new ArgumentException("ULID requires exactly 16 bytes (128 bits).", nameof(bytes));

            var chars = new char[UlidStringLength];

            // 128 bits -> 26 5-bit chunks (130 bits; top 2 bits are 0)
            // Byte 0-2 (24 bits -> chars 0..4)
            chars[0] = CrockfordBase32Alphabet[(bytes[0] & 0xE0) >> 5];
            chars[1] = CrockfordBase32Alphabet[bytes[0] & 0x1F];
            chars[2] = CrockfordBase32Alphabet[(bytes[1] & 0xF8) >> 3];
            chars[3] = CrockfordBase32Alphabet[((bytes[1] & 0x07) << 2) | ((bytes[2] & 0xC0) >> 6)];
            chars[4] = CrockfordBase32Alphabet[(bytes[2] & 0x3E) >> 1];

            // Byte 2-5 (chars 5..9)
            chars[5] = CrockfordBase32Alphabet[((bytes[2] & 0x01) << 4) | ((bytes[3] & 0xF0) >> 4)];
            chars[6] = CrockfordBase32Alphabet[((bytes[3] & 0x0F) << 1) | ((bytes[4] & 0x80) >> 7)];
            chars[7] = CrockfordBase32Alphabet[(bytes[4] & 0x7C) >> 2];
            chars[8] = CrockfordBase32Alphabet[((bytes[4] & 0x03) << 3) | ((bytes[5] & 0xE0) >> 5)];
            chars[9] = CrockfordBase32Alphabet[bytes[5] & 0x1F];

            // Byte 6-8 (chars 10..14)
            chars[10] = CrockfordBase32Alphabet[(bytes[6] & 0xF8) >> 3];
            chars[11] = CrockfordBase32Alphabet[((bytes[6] & 0x07) << 2) | ((bytes[7] & 0xC0) >> 6)];
            chars[12] = CrockfordBase32Alphabet[(bytes[7] & 0x3E) >> 1];
            chars[13] = CrockfordBase32Alphabet[((bytes[7] & 0x01) << 4) | ((bytes[8] & 0xF0) >> 4)];
            chars[14] = CrockfordBase32Alphabet[((bytes[8] & 0x0F) << 1) | ((bytes[9] & 0x80) >> 7)];

            // Byte 9-12 (chars 15..19)
            chars[15] = CrockfordBase32Alphabet[(bytes[9] & 0x7C) >> 2];
            chars[16] = CrockfordBase32Alphabet[((bytes[9] & 0x03) << 3) | ((bytes[10] & 0xE0) >> 5)];
            chars[17] = CrockfordBase32Alphabet[bytes[10] & 0x1F];
            chars[18] = CrockfordBase32Alphabet[(bytes[11] & 0xF8) >> 3];
            chars[19] = CrockfordBase32Alphabet[((bytes[11] & 0x07) << 2) | ((bytes[12] & 0xC0) >> 6)];

            // Byte 12-15 (chars 20..25)
            chars[20] = CrockfordBase32Alphabet[(bytes[12] & 0x3E) >> 1];
            chars[21] = CrockfordBase32Alphabet[((bytes[12] & 0x01) << 4) | ((bytes[13] & 0xF0) >> 4)];
            chars[22] = CrockfordBase32Alphabet[((bytes[13] & 0x0F) << 1) | ((bytes[14] & 0x80) >> 7)];
            chars[23] = CrockfordBase32Alphabet[(bytes[14] & 0x7C) >> 2];
            chars[24] = CrockfordBase32Alphabet[((bytes[14] & 0x03) << 3) | ((bytes[15] & 0xE0) >> 5)];
            chars[25] = CrockfordBase32Alphabet[bytes[15] & 0x1F];

            return new string(chars);
        }

        /// <summary>
        /// Validates whether a string is a well-formed 26-character Crockford Base32 ULID.
        /// </summary>
        public static bool IsValid(string ulid)
        {
            if (string.IsNullOrWhiteSpace(ulid) || ulid.Length != UlidStringLength)
                return false;

            for (var i = 0; i < ulid.Length; i++)
            {
                var c = char.ToUpperInvariant(ulid[i]);
                if (CrockfordBase32Alphabet.IndexOf(c) < 0)
                    return false;
            }

            return true;
        }
    }
}

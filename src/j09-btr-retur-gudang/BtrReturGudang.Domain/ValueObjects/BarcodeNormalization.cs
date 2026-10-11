using System;

namespace BtrReturGudang.Domain.ValueObjects
{
    /// <summary>
    /// Value Object implementing VO-01 Barcode Normalization rules.
    /// Traceable to: VO-01, TD-006, BGud BarcodeNormalization.
    ///
    /// Rules:
    /// 1. Strip CR ('\r'), LF ('\n'), and TAB ('\t').
    /// 2. Trim leading and trailing whitespace.
    /// 3. Strictly preserve leading zeros (string only, never parsed as numeric).
    /// 4. Lookup comparison key = UPPER(normalized value).
    /// </summary>
    public sealed class BarcodeNormalization : IEquatable<BarcodeNormalization>
    {
        public string RawValue { get; }
        public string NormalizedValue { get; }
        public string LookupKey { get; }

        public BarcodeNormalization(string rawValue)
        {
            RawValue = rawValue ?? string.Empty;
            NormalizedValue = Normalize(RawValue);
            LookupKey = ToLookupKey(RawValue);
        }

        /// <summary>
        /// Normalizes raw barcode input: strips CR, LF, TAB and trims whitespace,
        /// while preserving leading zeros.
        /// </summary>
        public static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Trim();
        }

        /// <summary>
        /// Derives the lookup key: UPPER(normalized value).
        /// </summary>
        public static string ToLookupKey(string value)
        {
            var normalized = Normalize(value);
            return normalized.ToUpperInvariant();
        }

        public bool IsEmpty => string.IsNullOrEmpty(NormalizedValue);

        public override string ToString() => NormalizedValue;

        public bool Equals(BarcodeNormalization other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(LookupKey, other.LookupKey, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is BarcodeNormalization other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (LookupKey != null ? StringComparer.Ordinal.GetHashCode(LookupKey) : 0);
        }

        public static bool operator ==(BarcodeNormalization left, BarcodeNormalization right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BarcodeNormalization left, BarcodeNormalization right)
        {
            return !Equals(left, right);
        }

        public static implicit operator string(BarcodeNormalization barcode)
        {
            return barcode?.NormalizedValue;
        }
    }
}

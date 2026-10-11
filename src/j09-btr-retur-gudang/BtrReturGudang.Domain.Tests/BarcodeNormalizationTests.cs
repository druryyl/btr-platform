using BtrReturGudang.Domain.ValueObjects;
using Xunit;

namespace BtrReturGudang.Domain.Tests
{
    public class BarcodeNormalizationTests
    {
        [Fact]
        public void Normalize_StripsCarriageReturnLineFeedAndTab()
        {
            var raw = "8992753\r\n\t";
            var normalized = BarcodeNormalization.Normalize(raw);

            Assert.Equal("8992753", normalized);
        }

        [Fact]
        public void Normalize_TrimsLeadingAndTrailingWhitespace()
        {
            var raw = "   8992753712345   ";
            var normalized = BarcodeNormalization.Normalize(raw);

            Assert.Equal("8992753712345", normalized);
        }

        [Fact]
        public void Normalize_StrictlyPreservesLeadingZeros()
        {
            var raw = "000123456789";
            var normalized = BarcodeNormalization.Normalize(raw);

            Assert.Equal("000123456789", normalized);
            Assert.StartsWith("000", normalized);
        }

        [Fact]
        public void ToLookupKey_ConvertsToUppercase()
        {
            var raw = " \t abc-00123 \r\n";
            var lookupKey = BarcodeNormalization.ToLookupKey(raw);

            Assert.Equal("ABC-00123", lookupKey);
        }

        [Fact]
        public void ValueObject_EqualityIsBasedOnLookupKey()
        {
            var vo1 = new BarcodeNormalization("abc12345\r\n");
            var vo2 = new BarcodeNormalization("  ABC12345\t");

            Assert.Equal(vo1, vo2);
            Assert.True(vo1 == vo2);
            Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());
        }

        [Fact]
        public void Normalize_HandlesNullAndEmpty()
        {
            Assert.Equal(string.Empty, BarcodeNormalization.Normalize(null));
            Assert.Equal(string.Empty, BarcodeNormalization.Normalize(""));
            Assert.Equal(string.Empty, BarcodeNormalization.Normalize("   \r\n\t   "));
        }
    }
}
